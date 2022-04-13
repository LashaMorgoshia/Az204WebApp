using Az204WebApp.Models;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace Az204WebApp.Helpers
{
    public static class StorageHelper
    {
        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
                return true;

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };
            return formats.Any(x => file.FileName.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig config)
        {
            try
            {
                var url = $"https://{config.AccountName}.blob.core.windows.net/{config.ImageContainer}/{fileName}";
                Uri blobUri = new Uri(url);

                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(config.AccountName, config.AccountKey);

                BlobClient blobClient = new BlobClient(blobUri);

                await blobClient.UploadAsync(fileStream);

                return await Task.FromResult(true);
            }
            catch(Exception ex) {
                var log = ex;
                return await Task.FromResult<bool>(false);
            }
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig config)
        {
            var urls = new List<string>();

            Uri accUri = new Uri($"https://{config.AccountName}.blob.core.windows.net/");

            BlobServiceClient blobServiceClient = new BlobServiceClient(accUri);

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(config.ImageContainer);

            if (blobContainerClient.Exists())
                urls.AddRange(blobContainerClient.GetBlobs().Select(x => $"{blobContainerClient.Uri }/{x.Name}").ToList());

            return await Task.FromResult(urls);
        }
    }
}
