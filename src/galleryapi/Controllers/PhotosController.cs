using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryAPI.Controllers
{
    public class PhotosController : ODataController
    {
        private readonly GalleryContext _context;

        public PhotosController(GalleryContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Ok(_context.Photos);
        }

        [EnableQuery]
        public IActionResult Get(string key)
        {
            return Ok(_context.Photos.FirstOrDefault(c => string.CompareOrdinal(c.PhotoId, key) == 0));
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
        public IActionResult Post([FromBody] Photo photo)
        {
            return Created(photo);
        }
    }
}
