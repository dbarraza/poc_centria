using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Models;
using Backend.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    /// <inheritdoc />
    public class ExcelService : BaseLogic<ExcelService>, IExcelService
    {
        private readonly IFileStorage _fileStorage;

        private readonly string _pendingExcelContainer;
        private const int SAS_TOKEN_EXPIRATION_IN_MINUTES = 60;

        public ExcelService(ISessionProvider sessionProvider, IDataAccess dataAccess, ILogger<ExcelService> logger, IConfiguration configuration, IFileStorage fileStorage) : base(sessionProvider, dataAccess, logger)
        {
            _pendingExcelContainer = configuration.GetValue<string>("blob:ExcelPendingFolder");
            _fileStorage = fileStorage;
        }

        /// <inheritdoc/>
        public async Task<Result<string>> UploadAsync(Guid applicationId, string fileName, Stream fileStream, string contentType)
        {
            try
            {
                // Get Application Entity
                var application = await dataAccess.Applications.GetAsync(applicationId);

                // Updload file to the storage
                var sasToken = _fileStorage.GetSasToken(_pendingExcelContainer, SAS_TOKEN_EXPIRATION_IN_MINUTES);
                var uri = await _fileStorage.SaveFileAsyncAsync(_pendingExcelContainer, applicationId.ToString(),  fileName, fileStream, contentType, Path.GetExtension(fileName));

                // Update entity with the uri
                application.ExcelUrl = uri;
                application.Status = ApplicationStatus.Pending;
                dataAccess.Applications.Update(application);
                await dataAccess.SaveChangesAsync();

                // Create Response
                return new Result<string> { Success = true, Message = "Document uploaded successfully.", Data = uri };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading document");
                return null;
            }
        }
    }
}
