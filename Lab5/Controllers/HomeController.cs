using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab5.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Lab5.Controllers
{
    public class HomeController : Controller
    {
        private PhotoDataContext _context;

        public HomeController(PhotoDataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View(_context.Photo.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileNow(ICollection<IFormFile> files)
        {

            // get your storage accounts connection string
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=xingstoragelab5;AccountKey=HoWdOuADQFn1Buhc/l8KNMbtRSYQiLbXm13tbk6Y0o92Jae7bLJE7UMYlWi7YlZhhzXfi0rJjV9zZ7+2H7uOYw==;EndpointSuffix=core.windows.net");

            // create an instance of the blob client
            var blobClient = storageAccount.CreateCloudBlobClient();

            // create a container to hold your blob (binary large object.. or something like that)
            // naming conventions for the curious https://msdn.microsoft.com/en-us/library/dd135715.aspx
            var container = blobClient.GetContainerReference("xingsphotostorage");
            await container.CreateIfNotExistsAsync();

            // set the permissions of the container to 'blob' to make them public
            var permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            await container.SetPermissionsAsync(permissions);

            // for each file that may have been sent to the server from the client
            foreach (var file in files)
            {
                try
                {
                    // create the blob to hold the data
                    var blockBlob = container.GetBlockBlobReference(file.FileName);
                    if (await blockBlob.ExistsAsync())
                        await blockBlob.DeleteAsync();

                    using (var memoryStream = new MemoryStream())
                    {
                        // copy the file data into memory
                        await file.CopyToAsync(memoryStream);

                        // navigate back to the beginning of the memory stream
                        memoryStream.Position = 0;

                        // send the file to the cloud
                        await blockBlob.UploadFromStreamAsync(memoryStream);
                    }

                    // add the photo to the database if it uploaded successfully
                    var photo = new Photo();
                    photo.Url = blockBlob.Uri.AbsoluteUri;
                    photo.FileName = file.FileName;

                    _context.Photo.Add(photo);
                    _context.SaveChanges();
                }
                catch
                {

                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeletePhoto(int id)
        {
            var photoToDelete = (from c in _context.Photo where c.PhotoId == id select c).FirstOrDefault();

            _context.Photo.Remove(photoToDelete);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}