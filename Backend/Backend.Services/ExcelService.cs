using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Models;
using Backend.Entities;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace Backend.Services
{
    /// <inheritdoc />
    public class ExcelService : BaseLogic, IExcelService
    {
        private readonly IFileStorage _fileStorage;
        private readonly BlobServiceClient _blobServiceClient;

        private const int SAS_TOKEN_EXPIRATION_IN_MINUTES = 60;
        private readonly int _modelDimensions = 1536;

        private readonly string _pendingExcelContainer;
        private readonly string _processedExcelContainer;
        private readonly string _errorExcelContainer;

        private readonly Uri _searchEndpoint;
        private readonly string _searchKey;
        private readonly string _indexName;
        private readonly string _vectorSearchProfileName;
        private readonly string _vectorSearchHnswConfig;
        private readonly string _semanticConfigName;

        private readonly Uri _openAIEndpoint;
        private readonly string _openAIKey;
        private readonly string _openAIModelName;

        public ExcelService(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<IExcelService> logger, IConfiguration configuration, IFileStorage fileStorage) : base(sessionProvider, dataAccess, logger)
        {
            _fileStorage = fileStorage;
            _pendingExcelContainer = configuration.GetValue<string>("blob:ExcelPendingFolder");
            _processedExcelContainer = configuration.GetValue<string>("blob:ExcelProcessedFolder");
            _errorExcelContainer = configuration.GetValue<string>("blob:ExcelErrorFolder");

            _indexName = configuration.GetValue<string>("search:IndexName");
            _vectorSearchProfileName = configuration.GetValue<string>("search:VectorSearchProfileName");
            _vectorSearchHnswConfig = configuration.GetValue<string>("search:VectorSearchHnswConfig");
            _semanticConfigName = configuration.GetValue<string>("search:SemanticConfigName");
            _searchEndpoint = new Uri(configuration.GetValue<string>("search:Endpoint"));
            _searchKey = configuration.GetValue<string>("search:Key");

            _openAIEndpoint = new Uri(configuration.GetValue<string>("AzureOpenAIApiEndpoint"));
            _openAIKey = configuration.GetValue<string>("AzureOpenAIApiKey");
            _openAIModelName = configuration.GetValue<string>("AzureOpenAIEmbeddingModel");

            _blobServiceClient = new BlobServiceClient(configuration.GetValue<string>("StorageConnectionString"));
        }

        /// <inheritdoc/>
        public async Task<string> UploadAsync(Guid applicationId, string fileName, Stream fileStream, string contentType)
        {
            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                // Updload file to the storage
                var sasToken = _fileStorage.GetSasToken(_pendingExcelContainer, SAS_TOKEN_EXPIRATION_IN_MINUTES);
                var uri = await _fileStorage.SaveFileAsyncAsync(_pendingExcelContainer, applicationId.ToString(), fileNameWithoutExtension, fileStream, contentType, Path.GetExtension(fileName));

                return uri;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading document");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task ProcessFileAsync(string applicationId, string fileName, Stream file)
        {
            // Get Application Entity
            var applicationGuid = Guid.Parse(applicationId);
            var application = await dataAccess.Applications.GetAsync(applicationGuid);

            try
            {
                var _openAIClient = new OpenAIClient(_openAIEndpoint, new AzureKeyCredential(_openAIKey));

                // Create the index
                await CreateSearchIndexAsync();

                // Get candidates from the excel file
                var candidates = GetCandidates(applicationId, file, fileName);
                var totalCandidates = candidates.Count();

                var count = 0;
                foreach (var candidate in candidates)
                {
                    logger.LogInformation($"[ExcelService:ProcessFileAsync] - Embedding content for candidate {count++} of {totalCandidates}");

                    // Get the formItem content embeddings
                    EmbeddingsOptions contentEmbeddingsOptions = new(_openAIModelName, new string[] { CleanUpTextForEmbeddings(candidate.Content) });
                    Embeddings contentEmbeddings = await _openAIClient.GetEmbeddingsAsync(contentEmbeddingsOptions);
                    ReadOnlyMemory<float> contentVector = contentEmbeddings.Data[0].Embedding;

                    candidate.ContentVector = contentVector.ToArray();
                }

                logger.LogInformation($"[ExcelService:ProcessFileAsync] - Start Indexing candidatos for application {applicationId} - date {DateTime.Now}");
                SearchClient searchClient = new(_searchEndpoint, _indexName, new AzureKeyCredential(_searchKey));
                await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(candidates));
                logger.LogInformation($"[ExcelService:ProcessFileAsync] - End Indexing candidatos for application {applicationId} - date {DateTime.Now}");
                
                // Move the file to the processed folder
                logger.LogInformation($"[ExcelService:ProcessFileAsync] - Moving the file {fileName} to the processed folder for application {applicationId}. From {_pendingExcelContainer} to {_processedExcelContainer}");
                var blobFileName = $"{applicationId}/{fileName}";
                var newUri = await _fileStorage.CopyBlobAsync(blobFileName, blobFileName, _pendingExcelContainer, _processedExcelContainer);
                logger.LogInformation($"[ExcelService:ProcessFileAsync] - File {fileName} moved to the processed folder for application {applicationId}. New URI: {newUri}");

                // Update Appication Status
                application.Status = ApplicationStatus.CandidatesLoaded;
                application.ExcelUrl = newUri;
                dataAccess.Applications.Update(application);
                await dataAccess.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"[ExcelService:ProcessFileAsync] - Error processing document {ex.Message}");

                // Move the file to the error folder
                logger.LogInformation($"[ExcelService:ProcessFileAsync] - Moving the file {fileName} to the error folder for application {applicationId}. From {_pendingExcelContainer} to {_errorExcelContainer}");
                var blobFileName = $"{applicationId}/{fileName}";
                await _fileStorage.CopyBlobAsync(blobFileName, blobFileName, _pendingExcelContainer, _errorExcelContainer);

                application.Status = ApplicationStatus.Error;
                application.ErrorMessage = ex.Message;
                dataAccess.Applications.Update(application);
            }
        }

        /// <summary>
        /// Get the URI for the blob including the SAS token
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private Uri GetFileUri(string folder, string fileName, string containerName)
        {
            // Get the SAS token for the container
            string sasToken = _fileStorage.GetSasToken(containerName, SAS_TOKEN_EXPIRATION_IN_MINUTES);

            // Construct the URI for the blob including the SAS token
            UriBuilder uriBuilder = new()
            {
                Scheme = "https", // Use HTTPS scheme for secure communication
                Host = $"{_blobServiceClient.AccountName}.blob.core.windows.net", // Specify the Azure Blob Storage account host
                Path = $"{folder}/{containerName}/{fileName}", // Combine the container name and file name to form the path
                Query = sasToken // Append the SAS token as query parameter to the URI
            };

            return uriBuilder.Uri; // Return the constructed URI
        }

        /// <summary>
        /// Get the candidates collection from the excel file
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        private IEnumerable<CandidateModel> GetCandidates(string applicationId, Stream fileStream, string fileName)
        {
            try
            {
                logger.LogInformation($"[ExcelService:GetCandidates] - Reading the excel file {fileName} for application {applicationId}");
                var candidates = new List<CandidateModel>();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Cargamos el archivo Excel
                using (var package = new ExcelPackage(fileStream))
                {
                    // Accedemos a la primera hoja del libro
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    // Obtenemos el número total de filas y columnas
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    int idColumnIndex = -1;
                    int nameColumnIndex = -1;
                    int emailColumnIndex = -1;
                    int salaryExpectationColumnIndex = -1;
                    int availabilityForWorkColumnIndex = -1;
                    int policeRecordColumnIndex = -1;
                    int criminalRecordColumnIndex = -1;
                    int judicialRecordColumnIndex = -1;
                    int consentColumnIndex = -1;
                    int hasFamiliarColumnIndex = -1;

                    // Recorremos la primera fila para buscar los encabezados
                    for (int col = 1; col <= colCount; col++)
                    {
                        string columnHeader = worksheet.Cells[1, col].Value?.ToString().Trim(); // Obtenemos el valor del encabezado

                        // Comparamos el valor del encabezado con los campos que buscamos
                        if (columnHeader == "ID")
                        {
                            idColumnIndex = col;
                        }
                        else if (columnHeader == "Nombres y Apellidos Completos")
                        {
                            nameColumnIndex = col;
                        }
                        else if (columnHeader == "Correo Electrónico de Contacto")
                        {
                            emailColumnIndex = col;
                        }
                        else if (columnHeader == "Expectativa Salarial (bruto mensual en Nuevos Soles):")
                        {
                            salaryExpectationColumnIndex = col;
                        }
                        else if (columnHeader == "¿Cuál es tu disponibilidad para ingresar a trabajar?")
                        {
                            availabilityForWorkColumnIndex = col;
                        }
                        else if (columnHeader == "¿Registras Antecedentes Policiales? (solo es informativo)")
                        {
                            policeRecordColumnIndex = col;
                        }
                        else if (columnHeader == "¿Registras Antecedentes Penales según lo dispuesto por la ley N°29607?")
                        {
                            criminalRecordColumnIndex = col;
                        }
                        else if (columnHeader == "¿Registras Antecedentes  Judiciales?")
                        {
                            judicialRecordColumnIndex = col;
                        }
                        else if (columnHeader.Contains("Dentro del proceso de selección, Centria en su calidad de potencial empleador, podrá realizar la consulta de algunos antecedentes del postulante incluidos aquellos brindados por centrales"))
                        {
                            consentColumnIndex = col;
                        }
                        else if (columnHeader == "¿Cuentas con vínculos y/o relación con algún miembro que actualmente labore en CENTRIA? (solo es informativo)")
                        {
                            hasFamiliarColumnIndex = col;
                        }
                    }

                    // Recorremos las filas restantes para obtener los valores
                    for (int row = 1 + 1; row < rowCount; row++)
                    {
                        var candidate = new CandidateModel();

                        for (int col = 1; col <= colCount; col++)
                        {
                            candidate.ApplicationId = applicationId.ToString();

                            // Accedemos al valor de la celda actual
                            string cellValue = worksheet.Cells[row, col].Value?.ToString();

                            if (col == idColumnIndex)
                            {
                                candidate.CandidateId = cellValue;
                            }
                            else if (col == nameColumnIndex)
                            {
                                candidate.Name = cellValue;
                            }
                            else if (col == emailColumnIndex)
                            {
                                candidate.Email = cellValue;
                            }
                            else if (col == salaryExpectationColumnIndex)
                            {
                                candidate.SalaryExpectation = int.Parse(cellValue);
                            }
                            else if (col == availabilityForWorkColumnIndex)
                            {
                                candidate.AvailabilityForWork = cellValue;
                            }
                            else if (col == policeRecordColumnIndex)
                            {
                                candidate.PoliceRecord = cellValue;
                            }
                            else if (col == criminalRecordColumnIndex)
                            {
                                candidate.CriminalRecord = cellValue;
                            }
                            else if (col == judicialRecordColumnIndex)
                            {
                                candidate.JudicialRecord = cellValue;
                            }
                            else if (col == consentColumnIndex)
                            {
                                candidate.Consent = cellValue;
                            }
                            else if (col == hasFamiliarColumnIndex)
                            {
                                candidate.HasFamiliar = cellValue;
                            }

                            else
                            {
                                string columnHeader = worksheet.Cells[1, col].Value?.ToString();
                                candidate.Content += $"{columnHeader} : {cellValue} | ";
                            }
                        }
                        candidates.Add(candidate);
                    }
                }

                logger.LogInformation($"[ExcelService:GetCandidates] - Candidates readed from the excel file {fileName} for application {applicationId}");
                return candidates;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[ExcelService:GetCandidates] - Error reading the excel file {fileName} for application {applicationId}. {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create a search index with a vector field and a vector search profile
        /// </summary>
        /// <returns></returns>
        private async Task CreateSearchIndexAsync()
        {
            var searchIndex = new SearchIndex(_indexName)
            {
                // Create fields based on the FormIndex model
                Fields =
                {
                    new SimpleField(nameof(CandidateModel.CandidateId), SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                    new SearchableField(nameof(CandidateModel.ApplicationId)) { IsFilterable = true, IsSortable = true },
                    new SearchableField(nameof(CandidateModel.Name)) { IsFilterable = true, IsSortable = true },
                    new SearchableField(nameof(CandidateModel.Email)) { IsFilterable = true, IsSortable = true },
                    new SearchableField(nameof(CandidateModel.Content)) { IsFilterable = true, IsSortable = true, IsFacetable = true, AnalyzerName = "es.Microsoft" },
                    new SimpleField(nameof(CandidateModel.SalaryExpectation), SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true, IsFacetable = true },
                    new SearchableField(nameof(CandidateModel.AvailabilityForWork)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new SearchableField(nameof(CandidateModel.PoliceRecord)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new SearchableField(nameof(CandidateModel.CriminalRecord)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new SearchableField(nameof(CandidateModel.JudicialRecord)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new SearchableField(nameof(CandidateModel.Consent)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new SearchableField(nameof(CandidateModel.HasFamiliar)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
                    new VectorSearchField(nameof(CandidateModel.ContentVector), _modelDimensions, _vectorSearchProfileName)
                },
                // Configure search based in vector similarity, for search similar documents or related documents
                VectorSearch = new()
                {
                    // The HNSW (Hierarchical Navigable Small World) algorithm constructs a hierarchical structure of nodes in high-dimensional vector space
                    // for efficient nearest neighbor search, providing scalability and accuracy for vector similarity search.
                    Algorithms =
                    {
                        new HnswAlgorithmConfiguration(_vectorSearchHnswConfig)
                    },
                    Profiles =
                    {
                        new VectorSearchProfile(_vectorSearchProfileName, _vectorSearchHnswConfig)
                    }
                },
                // Configure semantic search in specific fields
                SemanticSearch = new()
                {
                    Configurations =
                    {
                        new SemanticConfiguration(_semanticConfigName, new()
                        {
                            // Defines the title field to be used for semantic ranking, captions, highlights, and answers.
                            // If you don't have a title field in your index, leave this blank.
                            TitleField = new SemanticField(nameof(CandidateModel.Name)),

                            // Defines the content fields to be used for semantic ranking, captions, highlights, and answers.
                            // For the best result, the selected fields should contain text in natural language form
                            ContentFields =
                            {
                                new SemanticField(nameof(CandidateModel.Content))
                            }
                        })
                    }
                }
            };

            SearchIndexClient indexClient = new(_searchEndpoint, new AzureKeyCredential(_searchKey));
            await indexClient.CreateOrUpdateIndexAsync(searchIndex);
        }

        /// <summary>
        /// Clean up the text for embeddings
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string CleanUpTextForEmbeddings(string text)
        {
            var result = text;
            result = result.Replace("..", ".");
            result = result.Replace(". .", ".");
            result = result.Replace("\n", "");
            return result;
        }

    }
}
