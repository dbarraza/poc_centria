using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Entities;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    /// <inheritdoc/>
    public class ApplicationService : BaseLogic , IApplicationService
    {
        private readonly IExcelService _excelService;

        public ApplicationService(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<IApplicationService> logger, IExcelService excelService) : base(sessionProvider, dataAccess, logger)
        {
            _excelService = excelService;
        }

        public async Task<Application> CreateApplicationAsync(string applicationName, string fileName, Stream data, string contentType)
        {
            try
            {
                // Save entity in the database
                var application = new Application
                {
                    Id = Guid.NewGuid(),
                    Name = applicationName,
                    CreatedAt = DateTime.UtcNow,
                    Status = ApplicationStatus.Created
                };
                await dataAccess.Applications.InsertAsync(application);

                // Upload the file to the storage
                var uri = await _excelService.UploadAsync(application.Id, fileName, data, contentType);

                // Update the entity with the file uri
                application.ExcelUrl = uri;
                await dataAccess.SaveChangesAsync();

                return application;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, $"[ApplicationService:CreateApplicationAsync] - An error occurred while creating a new application with name: {applicationName}. Exception:{ex.Message}");
                throw;
            }
        }
    }
}
