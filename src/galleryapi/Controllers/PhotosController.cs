using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using System.IO;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GalleryAPI.Controllers
{
    public class PhotosController : ODataController
    {
        private readonly GalleryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhotosController(GalleryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            string userId = ControllerUtility.GetUserID(this._httpContextAccessor);

            var rst = _context.Photos.Where(p => p.IsPublic == true);
            return Ok(rst);
        }

        [EnableQuery]
        public IActionResult Get(string key)
        {
            return Ok(_context.Photos.FirstOrDefault(c => c.PhotoId == key));
        }

        /// <summary>
        /// If testing in IISExpress with the POST request to: http://localhost:2087/test/my/a/Customers
        /// Content-Type : application/json
        /// {
        ///    "Name": "Jonier","
        /// }
        /// 
        /// Check the reponse header, you can see 
        /// "Location" : "http://localhost:2087/test/my/a/Customers(0)"
        /// </summary>
        //[HttpPost]
        ////[EnableQuery]
        //public IActionResult Post([FromBody] Photo photo)
        //{
        //    return Forbid();
        //    //photo.UploadedTime = DateTime.Now;
        //    //this._context.Photos.Add(photo);
        //    //_context.SaveChanges();

        //    //return Created(photo);
        //}

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put(string key, [FromBody] Photo pto)
        {
            var entry = await _context.Photos.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            string userId = ControllerUtility.GetUserID(this._httpContextAccessor);
            if (userId == null)
            {
                return StatusCode(401);
            }

            entry.Desp = pto.Desp;
            entry.IsPublic = pto.IsPublic;
            entry.Title = pto.Title;
            _context.Entry(entry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Updated(pto);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> Patch([FromODataUri] string key, [FromBody] Delta<Photo> patchPhoto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = ControllerUtility.GetUserID(this._httpContextAccessor);
            if (userId == null)
            {
                return StatusCode(401);
            }

            if (patchPhoto != null)
            {
                var entry = await _context.Photos.FindAsync(key);
                if (entry == null)
                {
                    return NotFound();
                }

                patchPhoto.Patch(entry);
                // patchPhoto.App(entry, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Entry(entry).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return new ObjectResult(entry);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(string key)
        {
            var entry = await _context.Photos.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            string userId = ControllerUtility.GetUserID(this._httpContextAccessor);
            if (userId == null)
            {
                return StatusCode(401);
            }

            // Delete the file
            try
            {
                String strFullFile = Startup.UploadFolder + "\\" + entry.FileUrl;
                var trashFolder = Startup.UploadFolder + "\\Trash";
                if (!Directory.Exists(trashFolder))
                {
                    Directory.CreateDirectory(trashFolder);
                }
                if (System.IO.File.Exists(strFullFile))
                {
                    System.IO.File.Move(strFullFile, trashFolder + "\\" + entry.FileUrl);
                }
                strFullFile = Startup.UploadFolder + "\\" + entry.ThumbnailFileUrl;
                if (System.IO.File.Exists(strFullFile))
                {
                    System.IO.File.Move(strFullFile, trashFolder + "\\" + entry.ThumbnailFileUrl);
                }
            }
            catch
            {
                // DO nothing
            }

            // Delete DB entry
            _context.Photos.Remove(entry);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}
