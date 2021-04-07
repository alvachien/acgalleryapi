using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;

namespace GalleryAPI.Controllers
{
    public class AlbumPhotosController : ODataController
    {
        private readonly GalleryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlbumPhotosController(GalleryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.AlbumPhotos);
        }

        [HttpPost]
        [EnableQuery]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] AlbumPhoto albpto)
        {
            var entry = await _context.AlbumPhotos.FindAsync(albpto.AlbumID, albpto.PhotoID);
            if (entry != null)
            {
                return BadRequest();
            }

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

            _context.AlbumPhotos.Add(albpto);
            await _context.SaveChangesAsync();

            return Created(albpto);
        }

        [HttpDelete]
        [EnableQuery]
        [Authorize]

        public IActionResult Delete(int AlbumID, string PhotoID)
        {
            // Delete code here
            var entry = _context.AlbumPhotos.Find(AlbumID, PhotoID);
            if (entry == null)
            {
                return NotFound();
            }

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

            _context.AlbumPhotos.Remove(entry);
            _context.SaveChanges();

            return Ok();
        }
    }
}
