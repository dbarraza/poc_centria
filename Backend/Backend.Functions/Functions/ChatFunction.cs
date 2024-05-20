using Backend.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Functions.Functions
{
    public class ChatFunction
    {
        private readonly ILogger<ChatFunction> _logger;
        private readonly IChatService _chatService;
        private readonly IConfiguration _configuration;

        public ChatFunction(ILogger<ChatFunction> logger, IChatService chatService, IConfiguration configuration)
        {
            _logger = logger;
            _chatService = chatService;
            _configuration = configuration;
        }

        [Function("ChatFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
