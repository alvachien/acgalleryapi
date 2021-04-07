using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;

namespace GalleryAPI.Controllers
{
    public class PhotoTagsController : ODataController
    {
        private readonly GalleryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PhotoTagsController(GalleryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.PhotoTags);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] PhotoTag ptag)
        {
            string userId = null;
            try
            {
                userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception)
            {
                userId = null;
            }
            if (userId == null)
            {
                return StatusCode(401);
            }

            this._context.PhotoTags.Add(ptag);
            _context.SaveChanges();

            return Created(ptag);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] string keyPhotoID, [FromODataUri] string keyTagString)
        {
            string userId = null;
            try
            {
                userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception)
            {
                userId = null;
            }
            if (userId == null)
            {
                return StatusCode(401);
            }

            var entity = this._context.PhotoTags.Find(keyPhotoID, keyTagString);
            if (entity == null)
                return NotFound();

            this._context.PhotoTags.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
