using Backend.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Backend.Functions.Functions
{
    /// <summary>
    /// Function to process the curriculums vitae
    /// </summary>
    public class CvFunction
    {
        private readonly ILogger<CvFunction> _logger;
        private readonly ICvService _cvService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Receive all the dependencies by DI
        /// </summary>        
        public CvFunction(ICvService cvService, ILogger<CvFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _cvService = cvService;
            _configuration = configuration;
        }
    }
}