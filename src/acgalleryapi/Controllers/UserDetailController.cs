using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/UserDetail")]
    public class UserDetailController : Controller
    {
        // GET: api/UserDetail
        [HttpGet]
        public IEnumerable<string> Get()
        {
            String strSql = @"SELECT [UserID]
                          ,[DisplayAs]
                          ,[UploadFileMinSize]
                          ,[UploadFileMaxSize]
                          ,[AlbumCreate]
                          ,[AlbumChange]
                          ,[AlbumDelete]
                          ,[PhotoUpload]
                          ,[PhotoChange]
                          ,[PhotoDelete]
                          ,[AlbumRead]
                      FROM [dbo].[UserDetail];";
            return new string[] { "value1", "value2" };
        }

        // GET: api/UserDetail/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/UserDetail
        [HttpPost]
        public void Post([FromBody]string value)
        {
            String strSql = @"INSERT INTO [dbo].[UserDetail]
           ([UserID]
           ,[DisplayAs]
           ,[UploadFileMinSize]
           ,[UploadFileMaxSize]
           ,[AlbumCreate]
           ,[AlbumChange]
           ,[AlbumDelete]
           ,[PhotoUpload]
           ,[PhotoChange]
           ,[PhotoDelete]
           ,[AlbumRead])
     VALUES
           (<UserID, nvarchar(50),>
           ,<DisplayAs, nvarchar(50),>
           ,<UploadFileMinSize, int,>
           ,<UploadFileMaxSize, int,>
           ,<AlbumCreate, bit,>
           ,<AlbumChange, tinyint,>
           ,<AlbumDelete, tinyint,>
           ,<PhotoUpload, bit,>
           ,<PhotoChange, tinyint,>
           ,<PhotoDelete, tinyint,>
           ,<AlbumRead, tinyint,>)";
        }
        
        // PUT: api/UserDetail/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            string strSql = @"UPDATE [dbo].[UserDetail]
   SET [UserID] = <UserID, nvarchar(50),>
      ,[DisplayAs] = <DisplayAs, nvarchar(50),>
      ,[UploadFileMinSize] = <UploadFileMinSize, int,>
      ,[UploadFileMaxSize] = <UploadFileMaxSize, int,>
      ,[AlbumCreate] = <AlbumCreate, bit,>
      ,[AlbumChange] = <AlbumChange, tinyint,>
      ,[AlbumDelete] = <AlbumDelete, tinyint,>
      ,[PhotoUpload] = <PhotoUpload, bit,>
      ,[PhotoChange] = <PhotoChange, tinyint,>
      ,[PhotoDelete] = <PhotoDelete, tinyint,>
      ,[AlbumRead] = <AlbumRead, tinyint,>
 WHERE <Search Conditions,,>";
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            string strSql = @"DELETE FROM [dbo].[UserDetail]
      WHERE <Search Conditions,,>";
        }
    }
}
