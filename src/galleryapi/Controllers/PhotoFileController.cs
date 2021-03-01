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
using System.Data.Common;
using System.Data.SqlClient;
using System.Net;
using GalleryAPI.Models;
using ImageMagick;

namespace GalleryAPI.Controllers
{
    [Produces("application/json")]
    [Route("/PhotoFile")]
    public class PhotoFileController : Controller
    {
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<PhotoFileController> _logger;
        private IAuthorizationService _authorizationService;

        public PhotoFileController(IWebHostEnvironment env, ILogger<PhotoFileController> logger, IAuthorizationService authorizationService)
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
        [ResponseCache(Duration = 864000)]
        public IActionResult Get(string filename)
        {
            String strFullFile = Startup.UploadFolder + "\\" + filename;
            if (System.IO.File.Exists(strFullFile))
            {
                var image = System.IO.File.OpenRead(Startup.UploadFolder + "\\" + filename);
                return File(image, "image/jpeg");
            }

            return NotFound();
        }
        
        // POST: PhotoFile
        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            if (Request.Form.Files.Count <= 0)
                return BadRequest("No Files");

            // Only care about the first file
            var file = Request.Form.Files[0];

            var fileSize = file.Length;
            var filename1 = file.FileName;
            var idx1 = filename1.LastIndexOf('.');
            var fileext = filename1.Substring(idx1);

            var filerst = new PhotoFileSuccess();
            // Copy file to uploads folder
            filerst.deleteType = "DELETE";
            var randomFileName = Guid.NewGuid().ToString("N");
            filerst.name = filename1;
            var targetfilename = randomFileName + fileext;
            filerst.size = (int)fileSize;
            filerst.url = "PhotoFile/" + targetfilename;
            filerst.thumbnailUrl = "PhotoFile/" + randomFileName + ".thumb" + fileext;
            filerst.deleteUrl = filerst.url;


            PhotoFileErrorResult errrst = null;
            PhotoFileSuccessResult succrst = new PhotoFileSuccessResult();
            try
            {
                var filePath = Path.Combine(Startup.UploadFolder, targetfilename);
                var thmFilePath = Path.Combine(Startup.UploadFolder, randomFileName + ".thumb" + fileext);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);

                    using (IMagickImage image = new MagickImage(filePath))
                    {
                        var bThumbnailCreated = false;

                        // Retrieve the exif information
                        ExifProfile profile = (ExifProfile)image.GetExifProfile();
                        if (profile != null)
                        {
                            using (IMagickImage thumbnail = profile.CreateThumbnail())
                            {
                                // Check if exif profile contains thumbnail and save it
                                if (thumbnail != null)
                                {
                                    thumbnail.Write(thmFilePath);
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

                            // Save the result
                            image.Write(thmFilePath);
                        }
                    }
                }

                succrst.files = new List<PhotoFileSuccess>();
                succrst.files.Append(filerst);
            }
            catch(Exception exp)
            {
                errrst = new PhotoFileErrorResult();
                var fileerr = new PhotoFileError();
                fileerr.error = exp.Message;
                fileerr.name = filename1;
                errrst.files = new List<PhotoFileError>();
                errrst.files.Append(fileerr);
            }

            //await AnalyzeFile(file, Path.Combine(Startup.UploadFolder, rst.PhotoId + fileext), Path.Combine(Startup.UploadFolder, rst.PhotoId + ".thumb" + fileext), rst, usrName);
            if (errrst != null)
            {
                return new JsonResult(errrst);
            }
            return new JsonResult(filerst);
        }

        // PUT: api/PhotoFile/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }
        
//        // DELETE: api/ApiWithActions/5
//        [HttpDelete("{strfile}")]
//        [Authorize]
//        public IActionResult DeleteUploadedFile(String strfile)
//        {
//            var fileFullPath = Path.Combine(Startup.UploadFolder, strfile);
//            var filename = Path.GetFileNameWithoutExtension(fileFullPath);
//            var fileext = Path.GetExtension(fileFullPath);
//            var fileThumbFullPath = Path.Combine(Startup.UploadFolder, filename + ".thumb" + fileext);

//            try
//            {
//                // File
//                if (System.IO.File.Exists(fileFullPath))
//                {
//                    System.IO.File.Delete(fileFullPath);
//                }

//                // Thumbnail file
//                if (System.IO.File.Exists(fileThumbFullPath))
//                {
//                    System.IO.File.Delete(fileThumbFullPath);
//                }
//            }
//            catch (Exception exp)
//            {
//#if DEBUG
//                System.Diagnostics.Debug.WriteLine(exp.Message);
//#endif

//                return BadRequest(exp.Message);
//            }

//            return new ObjectResult(new PhotoViewModel());
//        }

//        private async Task<IActionResult> AnalyzeFile(IFormFile ffile, String filePath, String thmFilePath, PhotoViewModelEx updrst, String usrName)
//        {
//            Boolean bThumbnailCreated = false;

//            using (var fileStream = new FileStream(filePath, FileMode.Create))
//            {
//                await ffile.CopyToAsync(fileStream);

//                try
//                {
//                    ExifToolWrapper wrap = new ExifToolWrapper();
//                    wrap.Run(filePath);

//                    foreach (var item in wrap)
//                    {
//#if DEBUG
//                        System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", item.group, item.name, item.value);
//#endif
//                        if (item.group == "EXIF" || item.group == "Composite" || item.group == "XMP")
//                            updrst.ExifTags.Add(item);
//                    }
//                }
//                catch (Exception exp)
//                {
//#if DEBUG
//                    System.Diagnostics.Debug.WriteLine(exp.Message);
//#endif
//                    _logger.LogError(exp.Message);
//                }

//                try
//                {
//                    using (MagickImage image = new MagickImage(filePath))
//                    {
//                        updrst.Width = image.Width;
//                        updrst.Height = image.Height;

//                        // Retrieve the exif information
//                        ExifProfile profile = image.GetExifProfile();
//                        if (profile != null)
//                        {
//                            using (MagickImage thumbnail = profile.CreateThumbnail())
//                            {
//                                // Check if exif profile contains thumbnail and save it
//                                if (thumbnail != null)
//                                {
//                                    thumbnail.Write(thmFilePath);
//                                    updrst.ThumbWidth = thumbnail.Width;
//                                    updrst.ThumbHeight = thumbnail.Height;
//                                    bThumbnailCreated = true;
//                                }
//                            }
//                        }

//                        if (!bThumbnailCreated)
//                        {
//                            MagickGeometry size = new MagickGeometry(256, 256);
//                            // This will resize the image to a fixed size without maintaining the aspect ratio.
//                            // Normally an image will be resized to fit inside the specified size.
//                            size.IgnoreAspectRatio = false;

//                            image.Resize(size);
//                            updrst.ThumbWidth = image.Width;
//                            updrst.ThumbHeight = image.Height;

//                            // Save the result
//                            image.Write(thmFilePath);
//                        }
//                    }
//                }
//                catch (Exception exp)
//                {
//#if DEBUG
//                    System.Diagnostics.Debug.WriteLine(exp.Message);
//#endif
//                    _logger.LogError(exp.Message);

//                    updrst.success = false;
//                    updrst.error = exp.Message;
//                }
//            }

//            updrst.OrgFileName = ffile.FileName;
//            updrst.UploadedTime = DateTime.Now;
//            updrst.IsOrgThumbnail = bThumbnailCreated;
//            updrst.UploadedBy = usrName;            

//            return Json(true);
//        }
    }
}
