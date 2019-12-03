using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/Wakeup")]
    public class WakeupController : Controller
    {
        // GET: api/Wakeup
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "wake", "up" };
        }
    }
}
