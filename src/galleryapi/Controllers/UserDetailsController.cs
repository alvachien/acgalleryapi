using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GalleryAPI.Controllers
{
    public class UserDetailsController : ODataController
    {
        private readonly GalleryContext _context;

        public UserDetailsController(GalleryContext context)
        {
            _context = context;
        }

        [EnableQuery]
        [Authorize]
        public IActionResult Get(string key)
        {
            var rcd = _context.UserDetails.FirstOrDefault(c => c.UserID == key);
            if (rcd == null)
                return NotFound();

            return Ok(rcd);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] UserDetail usrdtl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }

            var userId = ControllerUtility.GetUserID(this);
            if (userId == null)
            {
                return StatusCode(401);
            }

            _context.UserDetails.Add(usrdtl);
            await _context.SaveChangesAsync();

            return Created(usrdtl);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put(string key, [FromBody] UserDetail usrdtl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state");
            }
            if (string.CompareOrdinal(key, usrdtl.UserID) != 0)
            {
                return BadRequest("Key is unmatched");
            }

            var userId = ControllerUtility.GetUserID(this);
            if (userId == null)
            {
                return StatusCode(401);
            }
            // Can not change other user
            if (String.CompareOrdinal(userId, key) != 0)
            {
                return StatusCode(401);
            }

            var entry = await _context.UserDetails.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            entry.DisplayAs = usrdtl.DisplayAs;
            entry.AlbumCreate = usrdtl.AlbumCreate;
            entry.PhotoUpload = usrdtl.PhotoUpload;
            entry.UploadFileMaxSize = usrdtl.UploadFileMaxSize;
            entry.UploadFileMinSize = usrdtl.UploadFileMinSize;            
            _context.Entry(entry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Updated(usrdtl);
        }
    }
}
