using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/PhotoTag")]
    public class PhotoTagController : Controller
    {
        // GET: api/PhotoTag
        [HttpGet]
        public IEnumerable<string> Get()
        {
            string strSql = @"SELECT [PhotoID]
      ,[Tag]
  FROM [dbo].[PhotoTag]";
            return new string[] { "value1", "value2" };
        }

        // GET: api/PhotoTag/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/PhotoTag
        [HttpPost]
        public void Post([FromBody]string value)
        {
            String strSql = @"INSERT INTO [dbo].[PhotoTag]
           ([PhotoID]
           ,[Tag])
     VALUES
           (<PhotoID, nvarchar(40),>
           ,<Tag, nvarchar(50),>)";
        }
        
        // PUT: api/PhotoTag/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            string strSql = @"UPDATE [dbo].[PhotoTag]
   SET [PhotoID] = <PhotoID, nvarchar(40),>
      ,[Tag] = <Tag, nvarchar(50),>
 WHERE <Search Conditions,,>";
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            string strSql = @"DELETE FROM [dbo].[PhotoTag]
      WHERE <Search Conditions,,>";
        }
    }
}
