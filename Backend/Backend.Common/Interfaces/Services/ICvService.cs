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
        Task<Result<List<ReceivedCv>>> GetReceivedCvsHistoryAsync(int page);

        /// <summary>
        /// Process a new Cv uploaded to the storage container.
        /// </summary>
        /// <returns></returns>      
        Task<bool> ProcessCvFromStorageAsync(Stream file, string filename);
    }


}
