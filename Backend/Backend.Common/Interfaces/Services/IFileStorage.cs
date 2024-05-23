namespace Backend.Common.Interfaces.Services
{
    public interface IFileStorage
    {
        /// <summary>
        /// Get the SAS token for the container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="expiresOnMinutes"></param>
        /// <returns></returns>
        public string GetSasToken(string container, int expiresOnMinutes);

        /// <summary>
        /// Save a file to the storage
        /// </summary>
        /// <param name="container"></param>
        /// <param name="fileName"></param>
        /// <param name="file"></param>
        /// <param name="contentType"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public Task<string> SaveFileAsyncAsync(string container, string folder, string fileName, Stream file, string contentType, string extension);
    }
}
