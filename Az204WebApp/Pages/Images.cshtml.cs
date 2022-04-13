using Az204WebApp.Helpers;
using Az204WebApp.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Az204WebApp.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly AzureStorageConfig config = null;

        [BindProperty]
        public IFormFile Upload { get; set; }

        public ImagesModel(IOptions<AzureStorageConfig> config)
        {
            this.config = config.Value;
        }

        public void OnGet()
        {
        }

        public async void OnPost()
        {
            try
            {
                if (StorageHelper.IsImage(Upload))
                {
                    if (Upload.Length > 0)
                        using (var stream = Upload.OpenReadStream())
                        {
                            // await StorageHelper.UploadFileToStorage(stream, Upload.FileName, storageConfig);

                            var url = $"https://{config.AccountName}.blob.core.windows.net/{config.ImageContainer}/{Upload.FileName}";
                            Uri blobUri = new Uri(url);

                            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(config.AccountName, config.AccountKey);

                            BlobClient blobClient = new BlobClient(blobUri);

                            BlobUploadOptions uploadOptions = new BlobUploadOptions()
                            {
                                TransferOptions = new StorageTransferOptions()
                                {
                                     MaximumTransferSize = 4 * 1024 * 1024,
                                     InitialTransferSize = 4 * 1024 * 1024,
                                }
                            };

                            await blobClient.UploadAsync(stream, uploadOptions);

                            await Task.FromResult(true);
                        }
                }
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
        }
    }
}
