using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Models;
using Backend.Entities;
using Backend.Models;
using Backend.Models.Out;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Backend.Services
{
    /// <inheritdoc/>
    public class ApplicationService : BaseLogic , IApplicationService
    {
        private readonly IExcelService _excelService;
        private readonly Uri _searchEndpoint;
        private readonly string _searchKey;
        private readonly string _indexName;

        public ApplicationService(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<IApplicationService> logger, IExcelService excelService, IConfiguration configuration) : base(sessionProvider, dataAccess, logger)
        {
            _excelService = excelService;
            _indexName = configuration.GetValue<string>("search:IndexName");
            _searchEndpoint = new Uri(configuration.GetValue<string>("search:Endpoint"));
            _searchKey = configuration.GetValue<string>("search:Key");
        }

        /// <inheritdoc/>
        public async Task<Application> CreateApplicationAsync(string applicationName, string fileName, Stream data, string contentType)
        {
            try
            {
                // Save entity in the database
                var application = new Application
                {
                    Id = Guid.NewGuid(),
                    Name = applicationName,
                    CreatedAt = DateTime.UtcNow,
                    Status = ApplicationStatus.Created
                };
                await dataAccess.Applications.InsertAsync(application);

                // Upload the file to the storage
                var uri = await _excelService.UploadAsync(application.Id, fileName, data, contentType);

                // Update the entity with the file uri
                application.ExcelUrl = uri;
                await dataAccess.SaveChangesAsync();

                return application;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"[ApplicationService:CreateApplicationAsync] - An error occurred while creating a new application with name: {applicationName}. Exception:{ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<ApplicationModelOut>>> GetApplicationsAsync(int page, int pageSize)
        {
            try
            {
                var applications = await dataAccess.Applications
                    .GetAsync(filter: null, orderBy: x => x.OrderByDescending(y => y.CreatedAt), includeProperties: string.Empty, page: page, pageSize: pageSize);

                var applicationsModel = applications.Select(x => new ApplicationModelOut
                {
                    Id = x.Id,
                    Name = x.Name,
                    ExcelUrl = x.ExcelUrl,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status.ToString()
                });

                return new Result<IEnumerable<ApplicationModelOut>> { Success = true, Data = applicationsModel };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"[ApplicationService:GetApplicationsAsync] - An error occurred while getting applications. Exception:{ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ApplicationModelOut>> GetApplicationByIdAsync(Guid applicationId)
        {
            try
            {
                var application = await dataAccess.Applications.GetAsync(applicationId);
                if(application == null)
                {
                    return null;
                }

                var applicationModel = new ApplicationModelOut
                {
                    Id = application.Id,
                    Name = application.Name,
                    ExcelUrl = application.ExcelUrl,
                    CreatedAt = application.CreatedAt,
                    Status = application.Status.ToString()
                };

                return new Result<ApplicationModelOut> { Success = true, Data = applicationModel };
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"[ApplicationService:GetApplicationByIdAsync] - An error occurred while getting application with id: {applicationId}. Exception:{ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<Result<IEnumerable<CandidateFilteredModelOut>>> PrefilterCandidatesAsync(Guid applicationId, string minSalaryExpect, string maxSalaryExpect, string policeRecord, string criminalRecord, string judicialRecord, string consent, string hasFamiliar, string query)
        {
            try
            {
                // Inicializar el cliente de búsqueda
                SearchClient searchClient = new(_searchEndpoint, _indexName, new AzureKeyCredential(_searchKey));

                // Construir la consulta de filtrado
                var filters = new List<string>();

                if (applicationId != Guid.Empty)
                {
                    filters.Add($"ApplicationId eq '{applicationId}'");
                }
                if (!string.IsNullOrEmpty(minSalaryExpect))
                {
                    filters.Add($"SalaryExpectation ge {minSalaryExpect}");
                }
                if (!string.IsNullOrEmpty(maxSalaryExpect))
                {
                    filters.Add($"SalaryExpectation le {maxSalaryExpect}");
                }
                if (!string.IsNullOrEmpty(policeRecord))
                {
                    filters.Add($"PoliceRecord eq '{policeRecord}'");
                }
                if (!string.IsNullOrEmpty(criminalRecord))
                {
                    filters.Add($"CriminalRecord eq '{criminalRecord}'");
                }
                if (!string.IsNullOrEmpty(judicialRecord))
                {
                    filters.Add($"JudicialRecord eq '{judicialRecord}'");
                }
                //if (!string.IsNullOrEmpty(consent))
                //{
                //    filters.Add($"Consent eq '{consent}'");
                //}
                if (!string.IsNullOrEmpty(consent))
                {
                    // Construye el patrón de búsqueda para la coincidencia parcial
                    string searchPattern = $"/.*{Regex.Escape(consent)}.*/";

                    // Agrega el filtro para buscar coincidencias parciales en el campo "Consent"
                    filters.Add($"search.ismatch('{searchPattern}', 'Consent')");
                }
                if (!string.IsNullOrEmpty(hasFamiliar))
                {
                    filters.Add($"HasFamiliar eq '{hasFamiliar}'");
                }

                string filterQuery = string.Join(" and ", filters);

                // Configurar las opciones de búsqueda
                var searchOptions = new SearchOptions
                {
                    Filter = filterQuery,
                    Size = 15,
                    IncludeTotalCount = true,
                    Select =
                    {
                        nameof(CandidateFilteredModelOut.ApplicationId),
                        nameof(CandidateFilteredModelOut.CandidateId),
                        nameof(CandidateFilteredModelOut.Name),
                        nameof(CandidateFilteredModelOut.Email),
                        nameof(CandidateFilteredModelOut.SalaryExpectation),
                        nameof(CandidateFilteredModelOut.AvailabilityForWork),
                        nameof(CandidateFilteredModelOut.PoliceRecord),
                        nameof(CandidateFilteredModelOut.CriminalRecord),
                        nameof(CandidateFilteredModelOut.JudicialRecord),
                        nameof(CandidateFilteredModelOut.Consent),
                        nameof(CandidateFilteredModelOut.HasFamiliar),
                        nameof(CandidateFilteredModelOut.Content)
                    },
                    // TODO: validar
                    //QueryType = SearchQueryType.Semantic,
                    //SemanticSearch = new SemanticSearchOptions
                    //{
                    //    SemanticConfigurationName = "cv-semantic-config",
                    //    QueryCaption = new QueryCaption(QueryCaptionType.Extractive)
                    //}
                };

                var searchQuery = string.IsNullOrEmpty(query) ? "*" : $"\"{query}\"";

                logger.LogInformation($"[ApplicationService:PrefilterCandidatesAsync] - Search query: {filterQuery}");

                // Ejecutar la búsqueda
                var queryResponse = await searchClient.SearchAsync<CandidateModel>(searchQuery, searchOptions);
                var queryResults = queryResponse.Value.GetResults().ToList();

                var candidatesModel = queryResults.Select(result =>
                {
                    //var highlights = result.SemanticSearch.Captions.FirstOrDefault()?.Text;
                    var document = result.Document;

                    var extractedHighlights = string.Empty;
                    if (!string.IsNullOrEmpty(document.Content))
                    {
                        if (document.Content.Replace(" ", "").Contains(query.Replace(" ", "")))
                        {
                            extractedHighlights = query;
                        }
                    }

                    return new CandidateFilteredModelOut
                    {
                        ApplicationId = document.ApplicationId,
                        CandidateId = document.CandidateId,
                        Name = document.Name,
                        Email = document.Email,
                        SalaryExpectation = document.SalaryExpectation,
                        AvailabilityForWork = document.AvailabilityForWork,
                        PoliceRecord = document.PoliceRecord,
                        CriminalRecord = document.CriminalRecord,
                        JudicialRecord = document.JudicialRecord,
                        Consent = document.Consent,
                        HasFamiliar = document.HasFamiliar,
                        Highlights = extractedHighlights
                    };
                });

                logger.LogInformation($"[ApplicationService:PrefilterCandidatesAsync] - Found {queryResults.Count} candidates");

                return new Result<IEnumerable<CandidateFilteredModelOut>> { Success = true, Data = candidatesModel.OrderBy(x => x.CandidateId) };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[ApplicationService:PrefilterCandidatesAsync] - An error occurred while prefiltering candidates. Exception:{ex.Message}");
                throw;
            }
        }
    }
}
