using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using acgalleryapi.ViewModels;
using System.Net;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/UserDetail")]
    public class UserDetailController : Controller
    {
        // GET: api/UserDetail
        [HttpGet]
        public IActionResult Get()
        {
            return Forbid();
        }

        // GET: api/UserDetail/5
        [HttpGet("{userid}")]
        [Authorize]
        public async Task<IActionResult> Get(String userid)
        {
            if (String.IsNullOrEmpty(userid))
                return BadRequest("No user id provided");

            // Create it into DB
            // var usrName = User.FindFirst(c => c.Type == "sub").Value;
            var vmResult = new UserDetailViewModel();
            SqlConnection conn = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrorMsg = "";

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    String queryString = @"SELECT [UserID]
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
                      FROM [dbo].[UserDetail]
                      WHERE [UserID] = N'" + userid + "'";

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        Int32 idx = 0;
                        vmResult.UserID = reader.GetString(idx++);
                        vmResult.DisplayAs = reader.GetString(idx++);
                        if (!reader.IsDBNull(idx))
                            vmResult.UploadFileMinSize = reader.GetInt32(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.UploadFileMaxSize = reader.GetInt32(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.AlbumCreate = reader.GetBoolean(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.AlbumChange = (UserOperatorAuthEnum)reader.GetByte(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.AlbumDelete = (UserOperatorAuthEnum)reader.GetByte(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.PhotoUpload = reader.GetBoolean(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.PhotoChange = (UserOperatorAuthEnum)reader.GetByte(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.PhotoDelete = (UserOperatorAuthEnum)reader.GetByte(idx++);
                        else
                            ++idx;
                        if (!reader.IsDBNull(idx))
                            vmResult.AlbumRead = (UserOperatorAuthEnum)reader.GetByte(idx++);
                        else
                            ++idx;
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;
                }
            }
            catch (Exception exp)
            {
                errorCode = HttpStatusCode.InternalServerError;
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrorMsg = exp.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
            }

            if (errorCode != HttpStatusCode.OK)
            {
                switch (errorCode)
                {
                    case HttpStatusCode.Unauthorized:
                        return Unauthorized();
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.BadRequest:
                        return BadRequest();
                    default:
                        return StatusCode(500, strErrorMsg);
                }
            }

            return new ObjectResult(vmResult);
        }

        // POST: api/UserDetail
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]UserDetailViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
                // Additional checks
            }
            else
            {
                return BadRequest();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Check the user detail exist or not
                    String strSql = @"SELECT COUNT(*) FROM [dbo].[UserDetail] WHERE [UserID] = N'" + vm.UserID + "'";
                    SqlCommand cmd = new SqlCommand(strSql, conn);
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    Boolean bExist = false;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        bExist = reader.GetInt32(0) > 0;

                        if (bExist)
                            throw new Exception("User Info already exist");
                    }
                    reader.Close();
                    cmd.Dispose();

                    strSql = @"INSERT INTO [dbo].[UserDetail]
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
                                       (@UserID
                                       ,@DisplayAs
                                       ,@UploadFileMinSize
                                       ,@UploadFileMaxSize
                                       ,@AlbumCreate
                                       ,@AlbumChange
                                       ,@AlbumDelete
                                       ,@PhotoUpload
                                       ,@PhotoChange
                                       ,@PhotoDelete
                                       ,@AlbumRead )";
                    cmd = new SqlCommand(strSql, conn);
                    cmd.Parameters.AddWithValue("@UserID", vm.UserID);
                    cmd.Parameters.AddWithValue("@DisplayAs", vm.DisplayAs);
                    cmd.Parameters.AddWithValue("@UploadFileMinSize", vm.UploadFileMinSize.HasValue? (object) vm.UploadFileMinSize.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UploadFileMaxSize", vm.UploadFileMaxSize.HasValue ? (object)vm.UploadFileMaxSize.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@AlbumCreate", vm.AlbumCreate.HasValue? (object)vm.AlbumCreate.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@AlbumChange", vm.AlbumChange.HasValue? (object)vm.AlbumChange.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@AlbumDelete", vm.AlbumDelete.HasValue? (object)vm.AlbumDelete.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhotoUpload", vm.PhotoUpload.HasValue? (object)vm.PhotoUpload.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhotoChange", vm.PhotoChange.HasValue? (object)vm.PhotoChange.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhotoDelete", vm.PhotoDelete.HasValue? (object)vm.PhotoDelete.Value: DBNull.Value);
                    cmd.Parameters.AddWithValue("@AlbumRead", vm.AlbumRead.HasValue? (object)vm.AlbumRead.Value: DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();

                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                return StatusCode(500, exp.Message);
            }

            return new ObjectResult(vm);
        }
        
        // PUT: api/UserDetail/5
        [HttpPut("{id}")]
        [Authorize]
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
        [Authorize]
        public void Delete(int id)
        {
            string strSql = @"DELETE FROM [dbo].[UserDetail]
                WHERE <Search Conditions,,>";
        }
    }
}
