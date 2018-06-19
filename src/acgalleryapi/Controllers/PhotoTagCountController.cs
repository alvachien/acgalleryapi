using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using acgalleryapi.ViewModels;
using System.Data.SqlClient;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/PhotoTagCount")]
    public class PhotoTagCountController : Controller
    {
        // GET: api/PhotoTagCount
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<PhotoTagCountViewModel> listRst = new List<PhotoTagCountViewModel>();
            using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
            {
                String strsql = @"SELECT [Tag], COUNT([PhotoID]) AS TagCount FROM [dbo].[PhotoTag] GROUP BY [Tag] ";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(strsql, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PhotoTagCountViewModel vm = new PhotoTagCountViewModel();                        
                        vm.TagString = reader.GetString(0);
                        vm.Count = reader.GetInt32(1);
                        listRst.Add(vm);
                    }
                }
            }

            return new JsonResult(listRst);
        }

        // GET: api/PhotoTagCount/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }
        
        // POST: api/PhotoTagCount
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            return Forbid();
        }

        // PUT: api/PhotoTagCount/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            return Forbid();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Forbid();
        }
    }
}
