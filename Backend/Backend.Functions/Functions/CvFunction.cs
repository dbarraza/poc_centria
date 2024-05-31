using Backend.Common.Extensions;
using Backend.Common.Interfaces.Services;
using Backend.Common.Models;
using Backend.Models;
using Backend.Models.In;
using HttpMultipartParser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Backend.Services
{

    /// <summary>
    /// Documents backend API
    /// </summary>
    public class CvsProcessing
    {
        private readonly ILogger<CvsProcessing> logger;
        private readonly ICvService businessLogic;


        /// <summary>
        /// Receive all the depedencies by DI
        /// </summary>        
        public CvsProcessing(ICvService businessLogic, ILogger<CvsProcessing> logger)
        {
            this.logger = logger;
            this.businessLogic = businessLogic;
        }


        /// <summary>
        /// Returns the processed eails
        /// </summary>       
        [OpenApiOperation(operationId: "Cv History", tags:["Cv"], Description = "Returns the history of processed documents")]
        [OpenApiParameter(name: "applicationId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "applicationId", Description = "applicationId")]
        [OpenApiParameter(name: "page", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "page", Description = "page")]
        [OpenApiParameter(name: "pageSize", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "pageSize", Description = "pageSize")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Response<ReceivedCv[]>), Description = "List of received Cv")]
        [Function(nameof(GetReceivedCvsHistoryAsync))]
        public async Task<HttpResponseData> GetReceivedCvsHistoryAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "history")] HttpRequestData request)
        {
            var pageNumber = 1;
            if (request.Query.AllKeys.Contains("page"))
            {
                _ = int.TryParse(request.Query["page"], out pageNumber);
            }
            var applicaitonId = Guid.Parse(request.Query["applicationId"]);

            var result = await this.businessLogic.GetReceivedCvsHistoryAsync(pageNumber, applicaitonId);

            return await request.CreateResponseAsync(result, null);
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

        //Add function to upload a new CV, it receives the file and the application id
        [OpenApiOperation(operationId:"UploadCv", tags: ["Cv"], Description = "Upload a new Cv")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(CvUploadIn), Required = true, Description = "Document to upload")]
        [Function(nameof(UploadCv))]
        public async Task<IActionResult> UploadCv([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {
            try
            {
                logger.LogInformation($"[CvsProcessing:UploadCv] - Uploading a new Cv");

                var body = await MultipartFormDataParser.ParseAsync(request.Body);
                var file = body.Files[0];
                var applicationId = body.GetParameterValue("applicationId");

                var response = await businessLogic.UploadCv(applicationId, file.FileName, file.Data, file.ContentType);

                logger.LogInformation($"[CvsProcessing:UploadCv] - New CV uploaded for application {applicationId}");

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"[CvsProcessing:UploadCv] - Error uploading a new Cv: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
