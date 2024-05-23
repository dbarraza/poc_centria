using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Backend.Common.Interfaces.DataAccess;
using Backend.Entities;
using Backend.Models;
using Microsoft.Extensions.Configuration;

namespace Backend.DataAccess
{
    /// <inheritdoc/>
    public class DataAccess : IDisposable, IDataAccess
    {
        private DatabaseContext _context;
        private bool disposed = false;
        private string storageConnectionString;
        private readonly BlobServiceClient _blobServiceClient;

        /// <inheritdoc/>
        public IRepository<Application> Applications { get; }

        /// <inheritdoc/>
        public IRepository<ReceivedCv> ReceivedCvs { get; }


        /// <summary>
        /// Copia un blob de un contenedor a otro y elmina el original
        /// </summary>
        public DataAccess(IConfiguration configuration)
        {
            _context = new DatabaseContext(configuration);
            
            this._context.Database.EnsureCreated();
            storageConnectionString = configuration["StorageConnectionString"];
            _blobServiceClient = new BlobServiceClient(this.storageConnectionString);
            
            // Repositories
            Applications = new Repository<Application>(_context);
        }
        public async Task<string> CopyBlobAsync(string filename, string newFileName, string originalContainerName, string newContainerName)
        {
            try
            {
                BlobContainerClient originalContainerClient = this._blobServiceClient.GetBlobContainerClient(originalContainerName);
                BlobClient originalBlobClient = originalContainerClient.GetBlobClient(filename);
                // Verificar que el blob original existe
                if (!await originalBlobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"El blob '{filename}' no existe en el contenedor '{originalContainerName}'.");
                }

                BlobContainerClient newContainerClient = this._blobServiceClient.GetBlobContainerClient(newContainerName);
                BlobClient newBlobClient = newContainerClient.GetBlobClient(newFileName);

                var emlMemoryStream = new MemoryStream();
                await originalBlobClient.DownloadToAsync(emlMemoryStream);
                emlMemoryStream.Position = 0;

                await newBlobClient.UploadAsync(emlMemoryStream, true);
                await originalBlobClient.DeleteIfExistsAsync();

                // Devuelve el URI del blob
                return newBlobClient.Uri.ToString();
            }
            catch (RequestFailedException ex)
            {
                // Manejo de excepciones específicas de Azure.Storage.Blobs
                Console.WriteLine($"Error al copiar el blob: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Otros tipos de excepciones
                Console.WriteLine($"Error desconocido al copiar el blob: {ex.Message}");
                throw;
            }
        }




        /// <inheritdoc/>
        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        /// <inheritdoc/>
        public string GetSasToken(string container, int expiresOnMinutes)
        {
            // Generates the token for this account
            var accountKey = string.Empty;
            var accountName = string.Empty;
            var connectionStringValues = this.storageConnectionString.Split(';')
                .Select(s => s.Split(new char[] { '=' }, 2))
                .ToDictionary(s => s[0], s => s[1]);
            if (connectionStringValues.TryGetValue("AccountName", out var accountNameValue) && !string.IsNullOrWhiteSpace(accountNameValue)
                && connectionStringValues.TryGetValue("AccountKey", out var accountKeyValue) && !string.IsNullOrWhiteSpace(accountKeyValue))
            {
                accountKey = accountKeyValue;
                accountName = accountNameValue;

                var storageSharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);
                var blobSasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = container,
                    ExpiresOn = DateTime.UtcNow + TimeSpan.FromMinutes(expiresOnMinutes)
                };

                blobSasBuilder.SetPermissions(BlobAccountSasPermissions.All);
                var queryParams = blobSasBuilder.ToSasQueryParameters(storageSharedKeyCredential);
                var sasToken = queryParams.ToString();
                return sasToken;
            }
            return string.Empty;
        }

        public async Task CreateStorageFolderIfNotExist(string folderName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(folderName);
            await containerClient.CreateIfNotExistsAsync();
        }
    }
}