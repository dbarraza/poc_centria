using Backend.Common.Interfaces.Services;
using Backend.Models.In;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
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
        [OpenApiOperation(operationId: "run", tags: ["Application"], Summary = "Create a new Application", Description = "Create a new Application", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(ExcelUploadIn), Required = true, Description = "Document to upload")]
        public async Task<IActionResult> CreateApplication([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {
            try
            {
                _logger.LogInformation($"[ChatFunction:CreateApplication] - Creating a new application");

                var body = await MultipartFormDataParser.ParseAsync(request.Body);
                var file = body.Files[0];
                var name = body.GetParameterValue("name");

                var response = await _applicationService.CreateApplicationAsync(name, file.FileName, file.Data, file.ContentType);

                _logger.LogInformation($"[ChatFunction:CreateApplication] - New application created with guid {response.Id}");

                return new OkObjectResult(response);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationFunction:CreateApplication] - Error creating a new application process: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
