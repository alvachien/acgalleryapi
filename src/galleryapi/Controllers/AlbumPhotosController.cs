using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace GalleryAPI.Controllers
{
    public class AlbumPhotosController : ODataController
    {
        private readonly GalleryContext _context;

        public AlbumPhotosController(GalleryContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.AlbumPhotos);
        }

        [HttpPost]
        [EnableQuery]
        public async Task<IActionResult> Post([FromBody] AlbumPhoto albpto)
        {
            var entry = await _context.AlbumPhotos.FindAsync(albpto.AlbumID, albpto.PhotoID);
            if (entry != null)
            {
                return BadRequest();
            }

            this._context.AlbumPhotos.Add(albpto);
            await _context.SaveChangesAsync();

            return Created(albpto);
        }

        [HttpDelete]
        [EnableQuery]
        public IActionResult Delete(int AlbumID, string PhotoID)
        {
            // Delete code here
            var entry = _context.AlbumPhotos.Find(AlbumID, PhotoID);
            if (entry == null)
            {
                return NotFound();
            }

            this._context.AlbumPhotos.Remove(entry);
            _context.SaveChanges();

            return Ok();
        }
    }
}
