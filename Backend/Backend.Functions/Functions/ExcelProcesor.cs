using Backend.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


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
    }
}