using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
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

    }
}
