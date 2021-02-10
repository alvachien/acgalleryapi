using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
