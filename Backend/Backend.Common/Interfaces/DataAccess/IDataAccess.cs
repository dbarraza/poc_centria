using Backend.Entities;
using Backend.Models;

namespace Backend.Common.Interfaces.DataAccess
{
    /// <summary>
    /// Data Access interface
    /// </summary>
    public interface IDataAccess
    {   
        /// <summary>
        /// Documents collection
        /// </summary>
        IRepository<ReceivedCv> ReceivedCvs { get; }

        /// <summary>
        /// Application collection
        /// </summary>
        IRepository<Application> Applications { get; }


        /// <summary>
        /// Clean up resources
        /// </summary>
        void Dispose();

        /// <summary>
        /// Saves all the changess
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
