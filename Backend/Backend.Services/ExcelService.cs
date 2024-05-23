using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Storage.Blobs;
using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Models;
using Backend.Entities;
using Backend.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        public async Task<Result<string>> UploadAsync(Guid applicationId, string fileName, Stream fileStream, string contentType)
        {
            try
            {
                // Get Application Entity
                var application = await dataAccess.Applications.GetAsync(applicationId);

                // Updload file to the storage
                var sasToken = _fileStorage.GetSasToken(_pendingExcelContainer, SAS_TOKEN_EXPIRATION_IN_MINUTES);
                var uri = await _fileStorage.SaveFileAsyncAsync(_pendingExcelContainer, applicationId.ToString(),  fileName, fileStream, contentType, Path.GetExtension(fileName));

                // Update entity with the uri
                application.ExcelUrl = uri;
                application.Status = ApplicationStatus.Pending;
                dataAccess.Applications.Update(application);
                await dataAccess.SaveChangesAsync();

                // Create Response
                return new Result<string> { Success = true, Message = "Document uploaded successfully.", Data = uri };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading document");
                return null;
            }
        }

        public async Task ProcessFileAsync(Guid applicationId, string fileName)
        {
            // Get Application Entity
            var application = await dataAccess.Applications.GetAsync(applicationId);

            Uri fileUri = GetFileUri(fileName, _pendingExcelContainer);            

            var openAIClient = new OpenAIClient(_openAIEndpoint, new AzureKeyCredential(_openAIKey));
        }

        /// <summary>
        /// Get the URI for the blob including the SAS token
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private Uri GetFileUri(string fileName, string containerName)
        {
            // Get the SAS token for the container
            string sasToken = _fileStorage.GetSasToken(containerName, SAS_TOKEN_EXPIRATION_IN_MINUTES);

            // Construct the URI for the blob including the SAS token
            UriBuilder uriBuilder = new()
            {
                Scheme = "https", // Use HTTPS scheme for secure communication
                Host = $"{_blobServiceClient.AccountName}.blob.core.windows.net", // Specify the Azure Blob Storage account host
                Path = $"{containerName}/{fileName}", // Combine the container name and file name to form the path
                Query = sasToken // Append the SAS token as query parameter to the URI
            };

            return uriBuilder.Uri; // Return the constructed URI
        }

        /// <summary>
        /// Create a search index with a vector field and a vector search profile
        /// </summary>
        /// <returns></returns>
        private async Task CreateSearchIndex(string applicationId)
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
                    new SearchableField(nameof(CandidateModel.SalaryExpectation)) { IsFilterable = true, IsSortable = true, IsFacetable = true},
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
    }
}
