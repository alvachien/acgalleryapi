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

namespace GalleryAPI.Controllers
{
    public class PhotoTagsController : ODataController
    {
        private readonly GalleryContext _context;

        public PhotoTagsController(GalleryContext context)
        {
            _context = context;
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
            this._context.PhotoTags.Add(ptag);
            _context.SaveChanges();

            return Created(ptag);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromODataUri] string keyPhotoID, [FromODataUri] string keyTagString)
        {
            var entity = this._context.PhotoTags.Find(keyPhotoID, keyTagString);
            if (entity == null)
                return NotFound();

            this._context.PhotoTags.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
