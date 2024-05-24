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
using System.IO;
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
        /// Process a new excel document
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filename"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        [Function(nameof(ProcessFileFromStorageAsync))]
        public async Task ProcessFileFromStorageAsync([BlobTrigger(blobPath: "excel-pending/{folder}/{filename}", Connection = "AzureWebJobsStorage")] Stream file, string folder, string filename)
        {
            try
            {
                await _excelService.ProcessFileAsync(folder, filename, file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ExcelFunction:ProcessFileFromStorageAsync] - Error processing document {ex.Message}");
            }
        }
    }
}