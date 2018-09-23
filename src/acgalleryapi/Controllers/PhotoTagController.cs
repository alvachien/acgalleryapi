using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using acgalleryapi.ViewModels;
using System.Net;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/PhotoTag")]
    public class PhotoTagController : Controller
    {
        // GET: api/PhotoTag
        [HttpGet]
        public async Task<IActionResult> Get([FromBody] List<String> photoids)
        {
            if (photoids.Count <= 0)
                return BadRequest("No photo list");

            List<PhotoTagViewModel> listRst = new List<PhotoTagViewModel>();
            using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
            {
                String strsql = @"SELECT [PhotoID],[Tag] FROM [dbo].[PhotoTag] WHERE [PhotoID] IN (";
                
                for(Int32 i = 0; i < photoids.Count; i ++)
                {
                    strsql += "N'" + photoids[i] + "'";
                    if (i != photoids.Count - 1)
                    {
                        strsql += ", ";
                    }
                }
                strsql += ") ";

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(strsql, conn);
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while(reader.Read())
                    {
                        PhotoTagViewModel vm = new PhotoTagViewModel();
                        vm.PhotoId = reader.GetString(0);
                        vm.TagString = reader.GetString(1);
                        listRst.Add(vm);
                    }
                }

                reader.Close();
                reader = null;
                cmd.Dispose();
                cmd = null;
            }

            return new JsonResult(listRst);
        }

        // GET: api/PhotoTag/5
        [HttpGet("{photoid}")]
        public IActionResult Get(string photoid)
        {
            //string strSql = @"SELECT [PhotoID],[Tag] FROM [dbo].[PhotoTag]";
            return Forbid();
        }
        
        // POST: api/PhotoTag
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            //String strSql = @"INSERT INTO [dbo].[PhotoTag] ([PhotoID],[Tag]) VALUES (<PhotoID, nvarchar(40),>
            //    ,<Tag, nvarchar(50),>)";
            return Forbid();
        }
        
        // PUT: api/PhotoTag/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            //string strSql = @"UPDATE [dbo].[PhotoTag]
            //           SET [PhotoID] = <PhotoID, nvarchar(40),>
            //              ,[Tag] = <Tag, nvarchar(50),>
            //         WHERE <Search Conditions,,>";
            return Forbid();
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            //string strSql = @"DELETE FROM [dbo].[PhotoTag] WHERE <Search Conditions,,>";
            return Forbid();
        }
    }
}
