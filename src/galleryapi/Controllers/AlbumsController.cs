using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;

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
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Ok(_context.Albums);
        }

        [AlbumEnableQuery]
        public IActionResult Get(int key)
        {
            return Ok(_context.Albums.FirstOrDefault(c => c.Id == key));
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
        [EnableQuery]
        public IActionResult Post([FromBody] Album album)
        {
            return Created(album);
        }

        [HttpGet]
        public IActionResult GetPhotos([FromODataUri]int albumid, string accesscode)
        {
            //if (parameters == null)
            //{
            //    throw new ODataException("Parameters is null");
            //}
            //if (parameters.Count == 0)
            //{
            //    throw new ODataException("Parameters is empty");
            //}
            //if (!parameters.ContainsKey("AlbumID"))
            //{
            //    return BadRequest();
            //}

            //// Album ID
            //var aid = (int)parameters.GetValueOrDefault("AlbumID");
            var album = _context.Albums.FirstOrDefault(c => c.Id == albumid);
            if (album == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(album.AccessCode))
            {
                if (string.CompareOrdinal(accesscode, album.AccessCode) != 0)
                {
                    return BadRequest("Access Code is wrong");
                }
            }

            var phts = from ap in _context.AlbumPhotos
                       join photo in _context.Photos
                       on ap.PhotoID equals photo.PhotoId
                       where ap.AlbumID == albumid
                       select photo;

            return Ok(phts);
        }
    }
}
