using Az204WebApp.Helpers;
using Az204WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Az204WebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageConfig storageConfig = null;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;
            try
            {

                if (files.Count == 0)
                    return BadRequest("No files to upload");

                if (string.IsNullOrEmpty(storageConfig.AccountKey) || string.IsNullOrEmpty(storageConfig.AccountName))
                    return BadRequest("Azure storage account is not configured");

                if (string.IsNullOrEmpty(storageConfig.ImageContainer))
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach(var file in files)
                {
                    if (StorageHelper.IsImage(file))
                    {
                        if (file.Length > 0)
                            using (var stream = file.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, file.Name, storageConfig);
                                if (!isUploaded)
                                    break;
                            }
                    }
                    else return new UnsupportedMediaTypeResult();
                }

                if (isUploaded)
                {
                    if (string.IsNullOrEmpty(storageConfig.ThumbnailContainer))
                        return new AcceptedResult();
                    else return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                }
                else return BadRequest("Look like the images couldn't upload to the storage");

            }
            catch(Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
                return new ObjectResult(thumbnailUrls);
            }
            catch(Exception ex) {
                return BadRequest(ex.Message);
            }
        }
    }
}
