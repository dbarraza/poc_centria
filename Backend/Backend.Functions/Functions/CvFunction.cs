using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Backend.Common.Interfaces.Services;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Backend.Common.Extensions;
using Backend.Common.Models;
using Backend.Common.Interfaces;
using Backend.Models;
using System.IO;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Linq;
namespace Backend.Services
{
    /// <inheritdoc />
    public class CvService : ICvService
    {
    }
    /// <summary>
    /// Documents backend API
    /// </summary>
    public class CvsProcessing
    {
        private readonly ILogger<CvsProcessing> logger;
        private readonly ICvsProcessingLogic businessLogic;


        /// <summary>
        /// Receive all the depedencies by DI
        /// </summary>        
        public CvsProcessing(ICvsProcessingLogic businessLogic, ILogger<CvsProcessing> logger)
        {
            this.logger = logger;
            this.businessLogic = businessLogic;
        }


        /// <summary>
        /// Returns the processed eails
        /// </summary>       
        [OpenApiOperation("History", new[] { "Cv" }, Description = "Returns the history of processed documents")]
        [OpenApiParameter("Authetication", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "User bearer token")]
        [OpenApiSecurity("X-Functions-Key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The function key to access the API")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Response<ReceivedCv[]>), Description = "List of received Cv")]
        [Function(nameof(GetReceivedCvsHistoryAsync))]
        public async Task<HttpResponseData> GetReceivedCvsHistoryAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "history")] HttpRequestData request)
        {
            var pageNumber = 1;
            if (request.Query.AllKeys.Contains("page"))
            {
                _ = int.TryParse(request.Query["page"], out pageNumber);
            }
            return await request.CreateResponse(this.businessLogic.GetReceivedCvsHistoryAsync, pageNumber, responseLinks =>
            {
                responseLinks.Links = [];
            }, logger);
        }


        /// <summary>
        /// Process a new Cv uploaded to the storage container.
        /// </summary>
        [Function(nameof(ProcessCvFromStorageAsync))]
        public async Task ProcessCvFromStorageAsync(
            [BlobTrigger("cv-sin-procesar/{filename}", Connection = "AzureWebJobsStorage")] Stream file,
            string filename)
        {
            await this.businessLogic.ProcessCvFromStorageAsync(file, filename);
        }

    }
}
