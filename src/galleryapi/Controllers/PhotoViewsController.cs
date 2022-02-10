using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GalleryAPI.Controllers
{
    public class PhotoViewsController : ODataController
    {
        private readonly GalleryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhotoViewsController(GalleryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.PhotoViews);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult SearchPhotoInAlbum(int AlbumID, string AccessCode = null)
        {
            Album selalb = null;

            // Is a logon user?
            string userId = null;
            try
            {
                userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception)
            {
                userId = null;
            }

            if (userId != null)
            {
                selalb = _context.Albums.FirstOrDefault(p => p.Id == AlbumID && (p.IsPublic == true || p.CreatedBy == userId));
            }
            else
            {
                selalb = _context.Albums.FirstOrDefault(c => c.Id == AlbumID);
                if (selalb != null)
                {
                    if (!string.IsNullOrEmpty(selalb.AccessCode))
                    {
                        if (AccessCode == null)
                        {
                            return BadRequest("Access code is required");
                        }

                        if (string.CompareOrdinal(AccessCode, selalb.AccessCode) != 0)
                        {
                            return BadRequest("Access Code is wrong");
                        }
                    }
                }
            }

            // Album ID
            if (selalb == null)
            {
                return NotFound();
            }

            var phts = from apv in _context.AlbumPhotoViews
                       where apv.AlbumID == AlbumID
                       select new PhotoView
                       {
                           PhotoId = apv.PhotoId,
                           Title = apv.Title,
                           Desp = apv.Desp,
                           Width = apv.Width,
                           Height = apv.Height,
                           ThumbWidth = apv.ThumbWidth,
                           ThumbHeight = apv.ThumbHeight,
                           FileUrl = apv.FileUrl,
                           ThumbnailFileUrl = apv.ThumbnailFileUrl,
                           UploadedTime = apv.UploadedTime,
                           UploadedBy = apv.UploadedBy,
                           OrgFileName = apv.OrgFileName,
                           IsOrgThumbnail = apv.IsOrgThumbnail,
                           IsPublic = apv.IsPublic,
                           CameraMaker = apv.CameraMaker,
                           CameraModel = apv.CameraModel,
                           LensModel = apv.LensModel,
                           AVNumber = apv.AVNumber,
                           ShutterSpeed = apv.ShutterSpeed,
                           ISONumber = apv.ISONumber,
                           Tags = apv.Tags
                       };

            return Ok(phts);
        }
    }
}
