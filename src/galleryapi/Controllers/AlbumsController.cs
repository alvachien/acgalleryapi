﻿using System.Collections.Generic;
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

            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj != null && !String.IsNullOrEmpty(usrObj.Value))
            {
                var albs = _context.Albums.Where(p => p.IsPublic == true && p.CreatedBy == usrObj.Value);
                return Ok(albs);
            }

            var rst = _context.Albums.Where(p => p.IsPublic == true);

            return Ok(rst);
        }

        [AlbumEnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_context.Albums.FirstOrDefault(c => c.Id == key));
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult GetPhotos(int AlbumID, string AccessCode = null)
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

        [HttpGet]
        [EnableQuery]
        public IActionResult GetRelatedPhotos(int key)
        {
            return GetRelatedPhotos(key, null);
        }

        [HttpPost]
        [Authorize]
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

            return Ok(true);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post([FromBody] Album album)
        {
            // Create new entries
            album.CreatedAt = DateTime.Now;

            this._context.Albums.Add(album);
            _context.SaveChanges();

            return Created(album);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put(int key, [FromBody] Album album)
        {
            var entry = await _context.Albums.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            entry.Desp = album.Desp;
            entry.IsPublic = album.IsPublic;
            entry.Title = album.Title;
            _context.Entry(entry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Updated(album);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(int key)
        {
            var entry = await _context.Albums.FindAsync(key);
            if (entry == null)
            {
                return NotFound();
            }

            _context.Albums.Remove(entry);
            await _context.SaveChangesAsync();

            return StatusCode(204); // HttpStatusCode.NoContent
        }
    }
}

