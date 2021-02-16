using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

namespace GalleryAPI.Controllers
{
    public class AlbumsController : ODataController
    {
        private readonly GalleryContext _context;

        public AlbumsController(GalleryContext context)
        {
            _context = context;
        }

        [AlbumEnableQuery]
        public IActionResult Get()
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Ok(_context.Albums);
        }

        [AlbumEnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_context.Albums.FirstOrDefault(c => c.Id == key));
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetPhotos(int AlbumID, string AccessCode)
        {
            // Album ID
            //var aid = (int)parameters.GetValueOrDefault("AlbumID");
            var album = _context.Albums.FirstOrDefault(c => c.Id == AlbumID);
            if (album == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(album.AccessCode))
            {
                if (AccessCode == null)
                {
                    return BadRequest("Access code is required");
                }

                if (string.CompareOrdinal(AccessCode, album.AccessCode) != 0)
                {
                    return BadRequest("Access Code is wrong");
                }
            }

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == AlbumID
                       select photo;

            return Ok(phts);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetPhotos(int AlbumID)
        {
            // Album ID
            //var aid = (int)parameters.GetValueOrDefault("AlbumID");
            var album = _context.Albums.FirstOrDefault(c => c.Id == AlbumID);
            if (album == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(album.AccessCode))
            {
                return BadRequest("Access code is required");
            }

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == AlbumID
                       select photo;

            return Ok(phts);
        }

        [HttpGet]
        public IActionResult GetRelatedPhotos(int key, string AccessCode)
        {
            // Album ID
            //var aid = (int)parameters.GetValueOrDefault("AlbumID");
            var album = _context.Albums.FirstOrDefault(c => c.Id == key);
            if (album == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(album.AccessCode))
            {
                if (AccessCode == null)
                {
                    return BadRequest("Access code is required");
                }

                if (string.CompareOrdinal(AccessCode, album.AccessCode) != 0)
                {
                    return BadRequest("Access Code is wrong");
                }
            }

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == key
                       select photo;

            return Ok(phts);
        }

        [HttpPost]
        public IActionResult ChangeAccessCode(int key, ODataActionParameters paras)
        {
            var album = _context.Albums.FirstOrDefault(c => c.Id == key);
            if (album == null)
            {
                return NotFound();
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

            return Ok(album);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Album album)
        {
            return Created(album);
        }

        [HttpPut]
        public IActionResult Put(int key, [FromBody] Album album)
        {
            return Updated(album);
        }
    }
}

