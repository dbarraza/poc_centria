using Backend.Common.Interfaces.Services;
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
        [Function("CreateApplication")]
        [OpenApiOperation(operationId: "run", tags: new[] { "Application" }, Summary = "Create a new Application", Description = "Create a new Application", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Name of the application", Description = "Name of the application")]
        public async Task<IActionResult> CreateApplication([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {
            try
            {
                _logger.LogInformation($"[ChatFunction:CreateApplication] - Creating a new application");

                var name = request.Query["name"];
                var response = await _applicationService.CreateApplicationAsync(name);

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
