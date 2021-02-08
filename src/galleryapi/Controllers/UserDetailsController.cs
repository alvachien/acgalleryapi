using System.Collections.Generic;
using System.Linq;
using GalleryAPI.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryAPI.Controllers
{
    public class UserDetailsController : ODataController
    {
        private readonly GalleryContext _context;

        public UserDetailsController(GalleryContext context)
        {
            _context = context;
        }
    }
}
