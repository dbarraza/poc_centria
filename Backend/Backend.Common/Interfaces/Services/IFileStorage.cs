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
        /// Get the SAS token for the file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetSasTokenFromBlob(string file, int expiresOnMinutes);

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

        /// <summary>
        /// Copy a blob from one container to another
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="newFileName"></param>
        /// <param name="originalContainerName"></param>
        /// <param name="newContainerName"></param>
        /// <returns></returns>
        public Task<string> CopyBlobAsync(string filename, string newFileName, string originalContainerName, string newContainerName);
    }
}
