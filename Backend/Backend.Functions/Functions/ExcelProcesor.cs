using Backend.Common.Extensions;
using Backend.Common.Services;
using Backend.Models.In;
using Backend.Models.Out;
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
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Receive all the dependencies by DI
        /// </summary>        
        public ExcelFunction(ILogger<ExcelFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
    }
}