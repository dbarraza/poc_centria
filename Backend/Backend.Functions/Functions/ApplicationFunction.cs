using Backend.Common.Extensions;
using Backend.Common.Interfaces.Services;
using Backend.Entities;
using Backend.Models;
using Backend.Models.In;
using Backend.Models.Out;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Functions.Functions
{
    public class ApplicationFunction
    {
        private readonly ILogger<ApplicationFunction> _logger;
        private readonly IApplicationService _applicationService;

        public ApplicationFunction(ILogger<ApplicationFunction> logger, IApplicationService applicationService)
        {
            _logger = logger;
            _applicationService = applicationService;
        }

        /// <summary>
        /// Create a new process of application
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Function(nameof(CreateApplication))]
        [OpenApiOperation(operationId: "CreateApplication", tags: ["Application"], Summary = "Create a new Application", Description = "Create a new Application", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(ExcelUploadIn), Required = true, Description = "Document to upload")]
        public async Task<IActionResult> CreateApplication([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {
            try
            {
                _logger.LogInformation($"[ChatFunction:CreateApplication] - Creating a new application");

                var body = await MultipartFormDataParser.ParseAsync(request.Body);
                var file = body.Files[0];
                var name = body.GetParameterValue("name");
                var jobDescription = body.GetParameterValue("jobDescription");

                var response = await _applicationService.CreateApplicationAsync(name, jobDescription, file.FileName, file.Data, file.ContentType);

                _logger.LogInformation($"[ChatFunction:CreateApplication] - New application created with guid {response.Id}");

                return new OkObjectResult(response);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationFunction:CreateApplication] - Error creating a new application process: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all Applications
        /// </summary>
        [Function(nameof(GetApplications))]
        [OpenApiOperation(operationId: "GetApplications", tags: ["Application"], Summary = "Get all application", Description = "Get all application", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiParameter(name: "page", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "page", Description = "page")]
        [OpenApiParameter(name: "pageSize", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "pageSize", Description = "pageSize")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApplicationModelOut[]), Summary = "Applications retrieved", Description = "The application to be retrieved")]
        public async Task<HttpResponseData> GetApplications([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, int page, int pageSize)
        {
            try
            {
                var result = await _applicationService.GetApplicationsAsync(page, pageSize);
                return await request.CreateResponseAsync(result, null);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationFunction:CreateApplication] - Error gettings applications process: {ex.Message}");
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Application
        /// </summary>
        [Function(nameof(GetApplication))]
        [OpenApiOperation(operationId: "GetApplication", tags: ["Application"], Summary = "Get application by id", Description = "Get application by id", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiParameter(name: "applicationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Summary = "applicationId", Description = "applicationId")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ApplicationModelOut), Summary = "Application retrieved", Description = "The application to be retrieved")]
        public async Task<HttpResponseData> GetApplication([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "application/{applicationId}")] HttpRequestData request, Guid applicationId)
        {
            try
            {
                var result = await _applicationService.GetApplicationByIdAsync(applicationId);
                return await request.CreateResponseAsync(result, null);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationFunction:CreateApplication] - Error gettings applications process: {ex.Message}");
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }


        [Function(nameof(GetPrefilteredCandidates))]
        [OpenApiOperation(operationId: "GetPrefilteredCandidates", tags: ["Application"], Summary = "Prefilter candidates", Description = "Prefilter candidates", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiParameter(name: "applicationId", In = ParameterLocation.Path, Required = true, Type = typeof(Guid), Summary = "applicationId", Description = "applicationId")]
        [OpenApiParameter(name: "minSalaryExpect", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "minSalaryExpect", Description = "minSalaryExpect")]
        [OpenApiParameter(name: "maxSalaryExpect", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "maxSalaryExpect", Description = "maxSalaryExpect")]
        [OpenApiParameter(name: "policeRecord", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "policeRecord", Description = "policeRecord")]
        [OpenApiParameter(name: "criminalRecord", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "criminalRecord", Description = "criminalRecord")]
        [OpenApiParameter(name: "judicialRecord", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "judicialRecord", Description = "judicialRecord")]
        [OpenApiParameter(name: "consent", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "consent", Description = "consent")]
        [OpenApiParameter(name: "hasFamiliar", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "hasFamiliar", Description = "hasFamiliar")]
        [OpenApiParameter(name: "query", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "query", Description = "query")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CandidateFilteredModelOut[]), Summary = "Prefiltered candidates retrieved", Description = "The prefiltered candidates to be retrieved")]
        public async Task<HttpResponseData> GetPrefilteredCandidates([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "application/{applicationId}/prefiltered-candidates")] HttpRequestData request, 
            Guid applicationId,
            string minSalaryExpect,
            string maxSalaryExpect,
            string policeRecord,  // No
            string criminalRecord, // No
            string judicialRecord, // No
            string consent, // Sí, estoy de acuerdo y otorgo mi consentimiento expreso e indubitable a través de este documento remitido a mi persona de forma virtual, para que Centria en su calidad de mi potencial empleador pueda realizar las consultas correspondientes, no presentando inconveniente alguno frente a ello.
            string hasFamiliar, // No, ningún tipo de relación
            string query)
        {
            try
            {
                var result = await _applicationService.PrefilterCandidatesAsync(applicationId, minSalaryExpect, maxSalaryExpect, policeRecord, criminalRecord, judicialRecord, consent, hasFamiliar, query);
                return await request.CreateResponseAsync(result, null);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationFunction:GetPrefilteredCandidates] - Error gettings prefiltered candidates process: {ex.Message}");
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}
