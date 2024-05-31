using Backend.Common.Models;
using Backend.Models;

namespace Backend.Common.Interfaces.Services
{
    /// <summary>
    /// Emial processing business logic
    /// </summary>
    public interface ICvService
    {
        /// <summary>
        /// Returns the history of processed Cvs
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Result<List<ReceivedCv>>> GetReceivedCvsHistoryAsync(int page, Guid applicationId);

        /// <summary>
        /// Process a new Cv uploaded to the storage container.
        /// </summary>
        /// <returns></returns>      
        Task<bool> ProcessCvFromStorageAsync(Stream file, string filename);

        /// <summary>
        /// Upload a Cv to the storage container
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<string> UploadCv(string applicationId, string fileName, Stream data, string contentType);
    }


}
