using Backend.Entities;
using Backend.Models;
using System;
using System.IO;
using System.Threading.Tasks;

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
        /// Clean up resources
        /// </summary>
        void Dispose();

        /// <summary>
        /// Saves all the changess
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Get sas token
        /// </summary>
        string GetSasToken(string container, int expiresOnMinutes);

        /// <summary>
        /// Creates a storage folder if it does not exist
        /// </summary>
        /// <param name="folerName"></param>
        /// <returns></returns>
        Task CreateStorageFolderIfNotExist(string folderName);
    }
}
