using Backend.Common.Extensions;
using Backend.Common.Interfaces.Services;
using Backend.Models.In;
using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;


namespace Backend.Functions.Functions
{
    /// <summary>
    /// Documents backend API
    /// </summary>
    public class ExcelFunction
    {
        private readonly ILogger<ExcelFunction> _logger;
        private readonly IExcelService _excelService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Receive all the dependencies by DI
        /// </summary>        
        public ExcelFunction(ILogger<ExcelFunction> logger, IConfiguration configuration, IExcelService excelService)
        {
            _logger = logger;
            _configuration = configuration;
            _excelService = excelService;
        }

        /// <summary>
        /// Upload a document to process
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Function(nameof(UploadDocument))]
        [OpenApiOperation(operationId: "run", tags: ["Prefilter"], Summary = "Upload document to process", Description = "Upload document to process", Visibility = OpenApiVisibilityType.Advanced)]
        [OpenApiSecurity("X-Functions-Key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The function key to access the API")]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(ExcelUploadIn), Required = true, Description = "Document to upload")]
        public async Task<HttpResponseData> UploadDocument([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents")] HttpRequestData request)
        {
            try
            {
                var body = await MultipartFormDataParser.ParseAsync(request.Body);
                var file = body.Files[0];
                var applicationId = Guid.Parse(body.GetParameterValue("applicationId"));

                var result = await _excelService.UploadAsync(applicationId, file.FileName, file.Data, file.ContentType);
                return await request.CreateResponseAsync(result, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}