using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assignment2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Assignment2.Controllers
{
    public class HomeController : Controller
    {
        private Assignment2DataContext _Assignment2DataContext;

        public HomeController(Assignment2DataContext context)
        {
            _Assignment2DataContext = context;
        }

        //[Route("[action]"), Route("")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("RoleId") == null)
            {
                HttpContext.Session.SetString("RoleId", "notLogin");
            }
            return View(_Assignment2DataContext.BlogPosts.Include(x => x.Photos).ToList());
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(Users user)
        {
            var existUser = (from u in _Assignment2DataContext.Users where u.EmailAddress == user.EmailAddress select u).FirstOrDefault();
            if (existUser == null)
            {
                if (Request.Form["User.RoleId"] == "1")
                {
                    user.RoleId = 1;
                }
                else
                {
                    user.RoleId = 2;
                }
                _Assignment2DataContext.Users.Add(user);
                _Assignment2DataContext.SaveChanges();
            }
            else
            {
                //ModelState.AddModelError("EmailAddress", "Email is alreay existing, please enter a new one");

                TempData["Exist"] = "EmailAddress";
                return RedirectToAction("Register");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EditProfile(int id)
        {
            var profileToUpdate = (from c in _Assignment2DataContext.Users where c.UserId == id select c).FirstOrDefault();
            return View(profileToUpdate);
        }

        [HttpPost]
        public IActionResult EditProfile(Users user)
        {
            var uid = Convert.ToInt32(Request.Form["UserId"]);

            var userProfile = (from c in _Assignment2DataContext.Users where c.UserId == uid select c).FirstOrDefault();

            try
            {
                userProfile.FirstName = user.FirstName;
                userProfile.LastName = user.LastName;
                userProfile.EmailAddress = user.EmailAddress;
                userProfile.Password = user.Password;
                userProfile.RoleId = user.RoleId;
                HttpContext.Session.SetString("RoleId", userProfile.RoleId.ToString());
                userProfile.DateOfBirth = user.DateOfBirth;
                userProfile.City = user.City;
                userProfile.Address = user.Address;
                userProfile.PostalCode = user.PostalCode;
                userProfile.Country = user.Country;
                _Assignment2DataContext.SaveChanges();
            }
            catch
            {
                return StatusCode(500);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UserLogin(Users user)
        {

            var _user = _Assignment2DataContext.Users.Where(a => a.EmailAddress.Equals(user.EmailAddress) && a.Password.Equals(user.Password)).FirstOrDefault();

            if (_user != null)
            {

                HttpContext.Session.SetString("UserId", _user.UserId.ToString());
                HttpContext.Session.SetString("FirstName", _user.FirstName.ToString());
                HttpContext.Session.SetString("LastName", _user.LastName.ToString());
                HttpContext.Session.SetString("RoleId", _user.RoleId.ToString());

                return RedirectToAction("Index");

            }
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult DisplayFullBlogPost(int id)
        {
            var subset = _Assignment2DataContext.BlogPosts.Include(x => x.Users).Include(d=>d.Photos).Include(y => y.Comments).ThenInclude(z => z.Users);
            var bid = (from c in subset where c.BlogPostId == id select c).FirstOrDefault();
            if (bid != null)
            {
                return View(bid);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult AddComment()
        {
            Comments com = new Models.Comments();
            com.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            com.BlogPostId = Convert.ToInt32(Request.Form["BlogPostId"]);
            com.Rating = Convert.ToInt32(Request.Form["Comment.Rating"]);
            com.Content = Request.Form["Comment.Content"];

            var badwordList = _Assignment2DataContext.BadWords.ToList();

            foreach (var word in badwordList)
            {
                if (com.Content.Contains(word.Word))
                    com.Content=com.Content.Replace(word.Word, "*****");
                
            }
            _Assignment2DataContext.Comments.Add(com);
            _Assignment2DataContext.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult EditBlog(int id)
        {
            var subset = _Assignment2DataContext.BlogPosts.Include(x => x.Photos);
            var blogToUpdate = (from c in subset where c.BlogPostId == id select c).FirstOrDefault();
            return View(blogToUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> ModifyBlog(BlogPosts item, ICollection<IFormFile> files)
        {
            var bid = Convert.ToInt32(Request.Form["BlogPostId"]);

            var blogToUpdate = (from c in _Assignment2DataContext.BlogPosts where c.BlogPostId == bid select c).FirstOrDefault();

            try
            {
                blogToUpdate.Title = item.Title;
                blogToUpdate.ShortDescription = item.ShortDescription;
                blogToUpdate.Content = item.Content;
                blogToUpdate.Posted = item.Posted;
                blogToUpdate.IsAvailable = item.IsAvailable;
                _Assignment2DataContext.SaveChanges();

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
                        var photo = new Photos();
                        photo.BlogPostId = bid;
                        photo.Url = blockBlob.Uri.AbsoluteUri;
                        photo.Filename = file.FileName;


                        _Assignment2DataContext.Photos.Add(photo);
                        _Assignment2DataContext.SaveChanges();
                    }
                    catch
                    {

                    }

                }

                return RedirectToAction("EditBlog", new { id = bid });
            }
            catch
            {
                return StatusCode(500);
            }
        
            
        }

        public IActionResult DeleteBlog(int id)
        {
            var blogToDelete = (from c in _Assignment2DataContext.BlogPosts where c.BlogPostId == id select c).FirstOrDefault();
            var commentlist = (from c in _Assignment2DataContext.Comments where c.BlogPostId == id select c).ToList();

            foreach (Comments com in commentlist)
            {
                _Assignment2DataContext.Comments.Remove(com);
                _Assignment2DataContext.SaveChanges();
            }

            _Assignment2DataContext.BlogPosts.Remove(blogToDelete);
            _Assignment2DataContext.SaveChanges();

            return RedirectToAction("Index");
        }


        public IActionResult Logout(Users user)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewBadWords()
        {
            return View(_Assignment2DataContext.BadWords.ToList());
        }

        [HttpPost]
        public IActionResult AddBadWords(BadWords item)
        {
            item.Word = Request.Form["BadWords.Word"];
            var existingWord = (from w in _Assignment2DataContext.BadWords where w.Word.ToLower() == item.Word.ToLower() select w).FirstOrDefault();

            if (existingWord == null)
            {
                _Assignment2DataContext.BadWords.Add(item);
                _Assignment2DataContext.SaveChanges();
                return RedirectToAction("ViewBadWords");
            }
            TempData["Exist"] = "BadWords";
            return RedirectToAction("ViewBadWords");
        }

        public IActionResult DeleteBadWord(int id)
        {
            var badWordToDelete = (from c in _Assignment2DataContext.BadWords where c.BadWordId == id select c).FirstOrDefault();

            _Assignment2DataContext.BadWords.Remove(badWordToDelete);
            _Assignment2DataContext.SaveChanges();

            return RedirectToAction("ViewBadWords");
        }

        [HttpGet]
        public IActionResult AddBlog()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog(BlogPosts blog, ICollection<IFormFile> files)
        {
            blog.Posted = DateTime.Now;
            blog.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            _Assignment2DataContext.BlogPosts.Add(blog);
            _Assignment2DataContext.SaveChanges();

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
                    var photo = new Photos();
                    photo.BlogPostId = blog.BlogPostId;
                    photo.Url = blockBlob.Uri.AbsoluteUri;
                    photo.Filename = file.FileName;
                    

                    _Assignment2DataContext.Photos.Add(photo);
                    _Assignment2DataContext.SaveChanges();
                }
                catch
                {

                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult DeletePhoto(int id)
        {
            var photoToDelete = (from c in _Assignment2DataContext.Photos where c.PhotoId == id select c).FirstOrDefault();

            _Assignment2DataContext.Photos.Remove(photoToDelete);
            _Assignment2DataContext.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}