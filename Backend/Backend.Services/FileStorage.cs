using Azure.Storage;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Backend.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Azure;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class FileStorage : IFileStorage
    {
        private readonly string _storageConnectionString;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<FileStorage> _logger;

        public FileStorage(IConfiguration configuration, ILogger<FileStorage> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("StorageConnectionString");
            _blobServiceClient = new BlobServiceClient(_storageConnectionString);
            _logger = logger;
        }

        // <inheritdoc />
        public string GetSasToken(string container, int expiresOnMinutes)
        {
            var accountKey = string.Empty;
            var accountName = string.Empty;
            var connectionStringValues = _storageConnectionString.Split(';')
                .Select(s => s.Split(['='], 2))
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

        // <inheritdoc />
        public async Task<string> SaveFileAsyncAsync(string container, string folder, string fileName, Stream file, string contentType, string extension)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var bloblClient = _blobServiceClient.GetBlobContainerClient(container);
            await bloblClient.CreateIfNotExistsAsync();

            var blobClient = bloblClient.GetBlobClient($"{folder}/{fileName}{extension}");
            file.Position = 0;

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders()
            };
            uploadOptions.HttpHeaders.ContentType = contentType;

            await blobClient.UploadAsync(file, uploadOptions);
            return blobClient.Uri?.ToString();
        }

        // <inheritdoc />
        public async Task<string> CopyBlobAsync(string filename, string newFileName, string originalContainerName, string newContainerName)
        {
            try
            {
                var bloblClient = _blobServiceClient.GetBlobContainerClient(newContainerName);
                await bloblClient.CreateIfNotExistsAsync();

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
                _logger.LogError(ex, $"[FileStorage:CopyBlobAsync] - Error al copiar el blob: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Otros tipos de excepciones
                _logger.LogError(ex, $"[FileStorage:CopyBlobAsync] - Error desconocido al copiar el blob: {ex.Message}");
                throw;
            }
        }
    }
}
