using Backend.Common.Models;

namespace Backend.Common.Interfaces.Services
{
    /// <summary>
    /// Service that manage the Excel processing
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// Upload a document to be processed
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public Task<Result<string>> UploadAsync(Guid applicationId, string fileName, Stream data, string contentType);

        /// <summary>
        /// Process a document
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        public Task ProcessFileAsync(Guid applicationId, string fileName);
    }
}
