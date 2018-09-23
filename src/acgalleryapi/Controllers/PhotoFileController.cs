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
using System.Data.Common;
using System.Data.SqlClient;
using System.Net;

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
        
        // POST: api/PhotoFile
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadPhotos(ICollection<IFormFile> files)
        {
            if (Request.Form.Files.Count <= 0)
                return BadRequest("No Files");

            // Only care about the first file
            var file = Request.Form.Files[0];
            //AuthorizationResult ar = await _authorizationService.AuthorizeAsync(User, file, "FileSizeRequirementPolicy");
            //if (ar.Succeeded)
            //{
            //}
            //else
            //{
            //    return BadRequest("File Size is not correct");
            //}

            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            Int32 minSize = 0, maxSize = 0; Boolean allowUpload = false;
            using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
            {
                await conn.OpenAsync();

                String cmdText = @"SELECT [UploadFileMinSize],[UploadFileMaxSize],[PhotoUpload]
                      FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrName + "'";
                SqlCommand cmdUserRead = new SqlCommand(cmdText, conn);
                SqlDataReader usrReader = await cmdUserRead.ExecuteReaderAsync();
                if (usrReader.HasRows)
                {
                    usrReader.Read();
                    if (!usrReader.IsDBNull(0))
                        minSize = usrReader.GetInt32(0);
                    if (!usrReader.IsDBNull(1))
                        maxSize = usrReader.GetInt32(1);
                    if (!usrReader.IsDBNull(2))
                        allowUpload = usrReader.GetBoolean(2);
                }

                usrReader.Close();
                usrReader = null;
                cmdUserRead.Dispose();
                cmdUserRead = null;
            }
            if (!allowUpload || maxSize == 0 || maxSize <= minSize)
            {
                return StatusCode(400, "User has no authoirty or wrongly set!");
            }
            // if (file.Length)
            var fileSize = file.Length / 1024;
            if (maxSize >= fileSize && minSize <= fileSize)
            {
                // Succeed
            }
            else
            {
                return StatusCode(400, "Wrong size!");
            }

            var rst = new PhotoViewModelEx(true);
            var filename1 = file.FileName;
            var idx1 = filename1.LastIndexOf('.');
            var fileext = filename1.Substring(idx1);

            rst.PhotoId = Guid.NewGuid().ToString("N");
            rst.FileUrl = rst.PhotoId + fileext;
            rst.ThumbnailFileUrl = rst.PhotoId + ".thumb" + fileext;

            await AnalyzeFile(file, Path.Combine(Startup.UploadFolder, rst.PhotoId + fileext), Path.Combine(Startup.UploadFolder, rst.PhotoId + ".thumb" + fileext), rst, usrName);

            return new JsonResult(rst);
        }

        // PUT: api/PhotoFile/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteUploadedFile(String strFile)
        {
            var fileFullPath = Path.Combine(Startup.UploadFolder, strFile);
            var filename = Path.GetFileNameWithoutExtension(fileFullPath);
            var fileext = Path.GetExtension(fileFullPath);
            var fileThumbFullPath = Path.Combine(Startup.UploadFolder, filename + ".thumb" + fileext);

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

        private async Task<IActionResult> AnalyzeFile(IFormFile ffile, String filePath, String thmFilePath, PhotoViewModelEx updrst, String usrName)
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
                        if (item.group == "EXIF" || item.group == "Composite" || item.group == "XMP")
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

                    updrst.success = false;
                    updrst.error = exp.Message;
                }
            }

            updrst.OrgFileName = ffile.FileName;
            updrst.UploadedTime = DateTime.Now;
            updrst.IsOrgThumbnail = bThumbnailCreated;
            updrst.UploadedBy = usrName;            

            return Json(true);
        }
    }
}
