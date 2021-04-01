using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using GalleryAPI.Models;
using ImageMagick;

namespace GalleryAPI.Controllers
{
    [Produces("application/json")]
    [Route("/PhotoFile")]
    public class PhotoFileController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<PhotoFileController> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly GalleryContext _context;

        public PhotoFileController(IWebHostEnvironment env, ILogger<PhotoFileController> logger, IAuthorizationService authorizationService,
            GalleryContext context)
        {
            _hostingEnvironment = env;
            _logger = logger;
            _authorizationService = authorizationService;
            _context = context;
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
        [Authorize]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            if (Request.Form.Files.Count <= 0)
                return BadRequest("No Files");
            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj == null || String.IsNullOrEmpty(usrObj.Value))
            {
                return StatusCode(401);
            }

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
            filerst.name = randomFileName;
            var targetfilename = randomFileName + fileext;
            filerst.size = (int)fileSize;
            // To avoid mass change the existing records in db, the URL won't return.
            // filerst.url = "PhotoFile/" + targetfilename;
            // filerst.thumbnailUrl = "PhotoFile/" + randomFileName + ".thumb" + fileext;
            filerst.url = targetfilename;
            filerst.thumbnailUrl = randomFileName + ".thumb" + fileext;
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
                        filerst.width = image.Width;
                        filerst.height = image.Height;

                        // Add the photo
                        var pht = new Photo();
                        pht.PhotoId = randomFileName;
                        pht.Title = pht.PhotoId;
                        pht.Desp = pht.PhotoId;
                        pht.FileUrl = filerst.url;
                        var exifprofile = image.GetExifProfile();
                        if (exifprofile != null)
                        {
                            // AV Number
                            IExifValue value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.ApertureValue);
                            try
                            {
                                if (value != null) pht.AVNumber = value.GetValue().ToString();
                            }
                            catch
                            {
                                // DO nothing
                            }
                            // Camera Maker
                            value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.Make);
                            try
                            {
                                if (value != null) pht.CameraMaker = value.GetValue().ToString();
                            }
                            catch
                            {
                                // DO nothing
                            }
                            // Camera Model
                            value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.Model);
                            try
                            {
                                if (value != null) pht.CameraModel = value.GetValue().ToString();
                            }
                            catch
                            {
                                // DO nothing
                            }
                            // ISO number
                            value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.ISOSpeed);
                            try
                            {
                                if (value != null) pht.ISONumber = (int)value.GetValue();
                            }
                            catch
                            {
                                // DO nothing
                            }
                            // Lens Model
                            value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.LensModel);
                            try
                            {
                                if (value != null) pht.LensModel = value.GetValue().ToString();
                            }
                            catch
                            {
                                // DO nothing
                            }
                            // Shutter Speed
                            value = exifprofile.Values.FirstOrDefault(val => val.Tag == ExifTag.ShutterSpeedValue);
                            try
                            {
                                if (value != null) pht.ShutterSpeed = (string)value.GetValue();
                            }
                            catch
                            {
                                // DO nothing
                            }
                        }

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

                                    filerst.thumbwidth = thumbnail.Width;
                                    filerst.thumbheight = thumbnail.Height;

                                    pht.ThumbnailFileUrl = filerst.thumbnailUrl;
                                    pht.ThumbHeight = filerst.thumbheight;
                                    pht.ThumbWidth = filerst.thumbwidth;
                                    pht.IsOrgThumbnail = true;
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
                            filerst.thumbwidth = image.Width;
                            filerst.thumbheight = image.Height;

                            pht.ThumbnailFileUrl = filerst.thumbnailUrl;
                            pht.ThumbHeight = filerst.thumbheight;
                            pht.ThumbWidth = filerst.thumbwidth;
                            pht.IsOrgThumbnail = false;

                            // Save the result
                            image.Write(thmFilePath);
                        }

                        pht.UploadedBy = usrObj.Value;
                        pht.UploadedTime = DateTime.Now;
                        this._context.Photos.Add(pht);

                        _context.SaveChanges();
                    }
                }

                succrst.files = new List<PhotoFileSuccess>();
                succrst.files.Append(filerst);
            }
            catch (Exception exp)
            {
                errrst = new PhotoFileErrorResult();
                var fileerr = new PhotoFileError();
                fileerr.error = exp.Message;
                fileerr.name = filename1;
                errrst.files = new List<PhotoFileError>();
                errrst.files.Append(fileerr);
            }

            if (errrst != null)
            {
                return new JsonResult(errrst);
            }
            return new JsonResult(filerst);
        }
    }
}
