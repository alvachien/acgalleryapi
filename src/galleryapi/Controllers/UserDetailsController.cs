using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
            return Ok(_context.UserDetails.FirstOrDefault(c => c.UserID == key));
        }
    }
}
