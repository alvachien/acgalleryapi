using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using acgalleryapi.ViewModels;
using System.Data.SqlClient;
using System.Net;

namespace acgalleryapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        // GET: api/Statistics
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var vmResult = new StatisticsViewModel();
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrorMsg = "";

            try
            {
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    String queryString = @"SELECT COUNT(*) FROM [dbo].[Album];
                        SELECT COUNT(*) FROM [dbo].[Photo];";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

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
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
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
    }
}
