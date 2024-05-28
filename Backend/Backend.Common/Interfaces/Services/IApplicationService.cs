using Backend.Common.Models;
using Backend.Entities;
using Backend.Models;
using Backend.Models.Out;

namespace Backend.Common.Interfaces.Services
{
    public interface IApplicationService
    {
        /// <summary>
        /// Create a new application
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public Task<Application> CreateApplicationAsync(string applicationName, string fileName, Stream data, string contentType);

        /// <summary>
        /// Get all applications
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Task<Result<IEnumerable<ApplicationModelOut>>> GetApplicationsAsync(int page, int pageSize);


        /// <summary>
        /// Get an application by id
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public Task<Result<ApplicationModelOut>> GetApplicationByIdAsync(Guid applicationId);
        
        /// <summary>
        /// Prefilter candidates based on the given parameters
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="minSalaryExpect"></param>
        /// <param name="maxSalaryExpect"></param>
        /// <param name="policeRecord"></param>
        /// <param name="criminalRecord"></param>
        /// <param name="judicialRecord"></param>
        /// <param name="consent"></param>
        /// <param name="hasFamiliar"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<Result<IEnumerable<CandidateFilteredModelOut>>> PrefilterCandidatesAsync(Guid applicationId, string minSalaryExpect, string maxSalaryExpect, string policeRecord, string criminalRecord, string judicialRecord, string consent, string hasFamiliar, string query);
    }
}
