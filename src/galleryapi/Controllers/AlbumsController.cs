using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GalleryAPI.Controllers
{
    public class AlbumsController : ODataController
    {
        private readonly GalleryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlbumsController(GalleryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [AlbumEnableQuery]
        public IActionResult Get()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj != null && !String.IsNullOrEmpty(usrObj.Value))
            {
                var albs = from almphoto in _context.AlbumPhotos
                           group almphoto by almphoto.AlbumID into almphotos
                           select new
                           {
                               ID = almphotos.Key,
                               PhotoCount = almphotos.Count()
                           } into almphotocnts
                           join alm in _context.Albums
                           on new { Id = almphotocnts.ID, IsAllowed = true } equals new { Id = alm.Id, IsAllowed = alm.IsPublic || alm.CreatedBy == usrObj.Value }
                           select new Album
                           {
                               Id = alm.Id,
                               Title = alm.Title,
                               Desp = alm.Desp,
                               CreatedBy = alm.CreatedBy,
                               CreatedAt = alm.CreatedAt,
                               IsPublic = alm.IsPublic,
                               AccessCodeHint = alm.AccessCodeHint,
                               PhotoCount = almphotocnts.PhotoCount
                           };

                return Ok(albs);
            }

            var rst2 = from almphoto in _context.AlbumPhotos
                               group almphoto by almphoto.AlbumID into almphotos
                               select new
                               {
                                   ID = almphotos.Key,
                                   PhotoCount = almphotos.Count()
                               } into almphotocnts
                       join alm in _context.Albums
                       on new  { Id = almphotocnts.ID, IsPublic = true } equals new { alm.Id, alm.IsPublic }
                       select new Album
                       {
                           Id = alm.Id,
                           Title = alm.Title,
                           Desp = alm.Desp,
                           CreatedBy = alm.CreatedBy,
                           CreatedAt = alm.CreatedAt,
                           IsPublic = alm.IsPublic,
                           AccessCodeHint = alm.AccessCodeHint,
                           PhotoCount = almphotocnts.PhotoCount
                       };

            return Ok(rst2);
        }

        [AlbumEnableQuery]
        public IActionResult Get(int key)
        {
            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj != null && !String.IsNullOrEmpty(usrObj.Value))
            {
                var alb = from almphoto in _context.AlbumPhotos
                           group almphoto by almphoto.AlbumID into almphotos
                           // where almphotos.Key equals usrObj.Value
                           select new
                           {
                               Key = almphotos.Key,
                               PhotoCount = almphotos.Count()
                           } into almphotocnts
                          join alm in _context.Albums
                          on new { Id = almphotocnts.Key, IsAllowed = true } equals new { Id = alm.Id, IsAllowed = alm.IsPublic || alm.CreatedBy == usrObj.Value }
                          where alm.Id == key
                          select new Album
                          {
                              Id = alm.Id,
                              Title = alm.Title,
                              Desp = alm.Desp,
                              CreatedBy = alm.CreatedBy,
                              CreatedAt = alm.CreatedAt,
                              IsPublic = alm.IsPublic,
                              AccessCodeHint = alm.AccessCodeHint,
                              PhotoCount = almphotocnts.PhotoCount
                          };
                if (alb.Count() != 1)
                    return NotFound();

                return Ok(alb.First());
            }

            var alb2 = from almphoto in _context.AlbumPhotos
                       group almphoto by almphoto.AlbumID into almphotos
                       // where almphotos.Key equals usrObj.Value
                       select new
                       {
                           Key = almphotos.Key,
                           PhotoCount = almphotos.Count()
                       } into almphotocnts
                       join alm in _context.Albums
                       on new { Id = almphotocnts.Key, IsPublic = true } equals new { alm.Id, alm.IsPublic }
                       where alm.Id == key
                       select new Album
                       {
                           Id = alm.Id,
                           Title = alm.Title,
                           Desp = alm.Desp,
                           CreatedBy = alm.CreatedBy,
                           CreatedAt = alm.CreatedAt,
                           IsPublic = alm.IsPublic,
                           AccessCodeHint = alm.AccessCodeHint,
                           PhotoCount = almphotocnts.PhotoCount
                       };

            if (alb2.Count() != 1)
                return NotFound();

            return Ok(alb2.First());
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetPhotos(int AlbumID, string AccessCode = null)
        {
            Album? selalb = null;
            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj != null && !String.IsNullOrEmpty(usrObj.Value))
            {
                selalb = _context.Albums.FirstOrDefault(p => p.Id == AlbumID && (p.IsPublic == true || p.CreatedBy == usrObj.Value));
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

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == AlbumID
                       select photo;

            return Ok(phts);
        }

        //[HttpGet]
        //[EnableQuery]
        //public IActionResult GetPhotos(int AlbumID)
        //{
        //    // Album ID
        //    //var aid = (int)parameters.GetValueOrDefault("AlbumID");
        //    var album = _context.Albums.FirstOrDefault(c => c.Id == AlbumID);
        //    if (album == null)
        //    {
        //        return NotFound();
        //    }

        //    if (!string.IsNullOrEmpty(album.AccessCode))
        //    {
        //        return BadRequest("Access code is required");
        //    }

        //    var phts = from ap in _context.AlbumPhotos
        //               join photo in _context.Photos
        //               on ap.PhotoID equals photo.PhotoId
        //               where ap.AlbumID == AlbumID
        //               select photo;

        //    return Ok(phts);
        //}

        [HttpGet]
        [EnableQuery]
        public IActionResult GetRelatedPhotos(int key, string AccessCode = null)
        {
            Album? selalb = null;

            string userId = null;
            try
            {
                userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch(Exception)
            {
                userId = null;
            }

            if (userId != null)
            {
                selalb = _context.Albums.FirstOrDefault(p => p.Id == key && (p.IsPublic == true || p.CreatedBy == userId));
            }
            else
            {
                selalb = _context.Albums.FirstOrDefault(c => c.Id == key);
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

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == key
                       select photo;

            return Ok(phts);
        }

        //[HttpGet]
        //[EnableQuery]
        //public IActionResult GetRelatedPhotos(int key)
        //{
        //    return GetRelatedPhotos(key, null);
        //}

        [HttpPost]
        [Authorize]
        public IActionResult ChangeAccessCode(int key, ODataActionParameters paras)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401);
            }

            var album = _context.Albums.FirstOrDefault(c => c.Id == key);
            if (album == null)
            {
                return NotFound();
            }

            if (String.CompareOrdinal(album.CreatedBy, userId) != 0)
            {
                return StatusCode(401);
            }

            var AccessCode = (string)paras["AccessCode"];
            if (string.IsNullOrEmpty(AccessCode))
            {
                album.AccessCodeHint = null;
            } 
            else
            {
                album.AccessCodeHint = AccessCode;
            }

            _context.Attach(album);
            _context.SaveChanges();

            return Ok(true);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Album album)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401);
            }

            // Create new entries
            album.CreatedAt = DateTime.Now;
            album.CreatedBy = userId;

            this._context.Albums.Add(album);
            _context.SaveChanges();

            return Created(album);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put(int key, [FromBody] Album album)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401);
            }

            var entry = await _context.Albums.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            if (String.CompareOrdinal(entry.CreatedBy, userId) != 0)
            {
                return StatusCode(401);
            }

            entry.Desp = album.Desp;
            entry.IsPublic = album.IsPublic;
            entry.Title = album.Title;            
            _context.Entry(entry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Updated(album);
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> Patch([FromODataUri] int key, [FromBody] Delta<Album> patchAlbum)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401);
            }

            if (patchAlbum != null)
            {
                var entry = await _context.Albums.FindAsync(key);
                if (entry == null)
                {
                    return NotFound();
                }
                if (String.CompareOrdinal(entry.CreatedBy, userId) != 0)
                {
                    return StatusCode(401);
                }

                patchAlbum.Patch(entry);

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
        public async Task<IActionResult> Delete(int key)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return StatusCode(401);
            }

            var entry = await _context.Albums.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            if (String.CompareOrdinal(entry.CreatedBy, userId) != 0)
            {
                return StatusCode(401);
            }

            _context.Albums.Remove(entry);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}

