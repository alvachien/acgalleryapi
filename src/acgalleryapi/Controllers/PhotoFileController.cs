using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using acgalleryapi.ViewModels;
using ImageMagick;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/PhotoFile")]
    public class PhotoFileController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<PhotoFileController> _logger;
        private IAuthorizationService _authorizationService;

        public PhotoFileController(IHostingEnvironment env, ILogger<PhotoFileController> logger, IAuthorizationService authorizationService)
        {
            _hostingEnvironment = env;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        // GET: api/PhotoFile
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET: api/PhotoFile/filename
        [HttpGet("{filename}")]
        public async Task<IActionResult> Get(string filename)
        {
            var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "/uploads");

            var image = System.IO.File.OpenRead(uploads + "\\" + filename);
            return File(image, "image/jpeg");
        }
        
        // POST: api/PhotoFile
        [HttpPost]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "/uploads");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            if (Request.Form.Files.Count <= 0)
                return BadRequest("No Files");

            // Only care about the first file
            var file = Request.Form.Files[0];
            AuthorizationResult ar = await _authorizationService.AuthorizeAsync(User, file, "FileSizeRequirementPolicy");
            if (ar.Succeeded)
            {
            }
            else
            {
                return BadRequest("File Size is not correct");
            }
            var usrName = User.FindFirst(c => c.Type == "sub").Value;

            var rst = new PhotoViewModel();
            var filename1 = file.FileName;
            var idx1 = filename1.LastIndexOf('.');
            var fileext = filename1.Substring(idx1);

            rst.PhotoId = Guid.NewGuid().ToString("N");
            rst.FileUrl = "/uploads/" + rst.PhotoId + fileext;
            rst.ThumbnailFileUrl = "/uploads/" + rst.PhotoId + ".thumb" + fileext;

            await AnalyzeFile(file, Path.Combine(uploads, rst.PhotoId + fileext), Path.Combine(uploads, rst.PhotoId + ".thumb" + fileext), rst, usrName);

            return new ObjectResult(rst);
        }

        // PUT: api/PhotoFile/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult DeleteUploadedFile(String strFile)
        {
            var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot/uploads");
            var fileFullPath = Path.Combine(uploads, strFile);
            var filename = Path.GetFileNameWithoutExtension(fileFullPath);
            var fileext = Path.GetExtension(fileFullPath);
            var fileThumbFullPath = Path.Combine(uploads, filename + ".thumb" + fileext);

            try
            {
                // File
                if (System.IO.File.Exists(fileFullPath))
                {
                    System.IO.File.Delete(fileFullPath);
                }

                // Thumbnail file
                if (System.IO.File.Exists(fileThumbFullPath))
                {
                    System.IO.File.Delete(fileThumbFullPath);
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif

                return BadRequest(exp.Message);
            }

            return new ObjectResult(new PhotoViewModel());
        }

        private async Task<IActionResult> AnalyzeFile(IFormFile ffile, String filePath, String thmFilePath, PhotoViewModel updrst, String usrName)
        {
            Boolean bThumbnailCreated = false;

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ffile.CopyToAsync(fileStream);

                try
                {
                    ExifToolWrapper wrap = new ExifToolWrapper();
                    wrap.Run(filePath);

                    foreach (var item in wrap)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", item.group, item.name, item.value);
#endif
                        if (item.group != "File")
                            updrst.ExifTags.Add(item);
                    }
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                    _logger.LogError(exp.Message);
                }

                try
                {
                    using (MagickImage image = new MagickImage(filePath))
                    {
                        updrst.Width = image.Width;
                        updrst.Height = image.Height;

                        // Retrieve the exif information
                        ExifProfile profile = image.GetExifProfile();
                        if (profile != null)
                        {
                            using (MagickImage thumbnail = profile.CreateThumbnail())
                            {
                                // Check if exif profile contains thumbnail and save it
                                if (thumbnail != null)
                                {
                                    thumbnail.Write(thmFilePath);
                                    updrst.ThumbWidth = thumbnail.Width;
                                    updrst.ThumbHeight = thumbnail.Height;
                                    bThumbnailCreated = true;
                                }
                            }
                        }

                        if (!bThumbnailCreated)
                        {
                            MagickGeometry size = new MagickGeometry(256, 256);
                            // This will resize the image to a fixed size without maintaining the aspect ratio.
                            // Normally an image will be resized to fit inside the specified size.
                            size.IgnoreAspectRatio = false;

                            image.Resize(size);
                            updrst.ThumbWidth = image.Width;
                            updrst.ThumbHeight = image.Height;

                            // Save the result
                            image.Write(thmFilePath);
                        }
                    }
                }
                catch (Exception exp)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                    _logger.LogError(exp.Message);
                }
            }

            updrst.UploadedTime = DateTime.Now;
            updrst.IsOrgThumbnail = bThumbnailCreated;
            updrst.UploadedBy = usrName;

            return Json(true);
        }
    }
}
