using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Entities;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    /// <inheritdoc/>
    public class ApplicationService : BaseLogic<ApplicationService> , IApplicationService
    {
        private readonly ILogger<ApplicationService> _logger;

        public ApplicationService(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<ApplicationService> logger) : base(sessionProvider, dataAccess, logger)
        {
        }

        public async Task<Application> CreateApplicationAsync(string name)
        {
            try
            {
                // Save entity in the database
                var application = new Application
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    Status = ApplicationStatus.Created
                };
                await dataAccess.Applications.InsertAsync(application);
                await dataAccess.SaveChangesAsync();

                return application;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"[ApplicationService:CreateApplicationAsync] - An error occurred while creating a new application with name: {name}. Exception:{ex.Message}");
                throw;
            }
        }
    }
}
