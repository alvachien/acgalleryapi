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

                    String queryString = @"
                        SELECT COUNT(*) FROM [dbo].[Album];
                        SELECT COUNT(*) FROM [dbo].[Photo];
                        SELECT TOP (5) [AlbumID], COUNT([PhotoID]) AS PhotoAmount from dbo.AlbumPhoto
                                GROUP BY [AlbumID]
                                ORDER BY PhotoAmount DESC;
                        SELECT TOP (5) Tag, COUNT(PhotoID) AS PhotoAmount FROM dbo.PhotoTag
	                        GROUP BY [Tag]
	                        ORDER BY PhotoAmount DESC;";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    // 1. Album count
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (!reader.IsDBNull(0))
                            vmResult.AlbumAmount = reader.GetInt32(0);
                    }
                    await reader.NextResultAsync();
                    // 2. Photo count
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (!reader.IsDBNull(0))
                            vmResult.PhotoAmount = reader.GetInt32(0);
                    }
                    await reader.NextResultAsync();
                    // 3. Top 5 albums with largest photo amount
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            if (!reader.IsDBNull(1))
                                vmResult.PhotoAmountInTop5Album.Add(reader.GetInt32(1));
                        }
                    }
                    await reader.NextResultAsync();
                    // 4. Top 5 tag with photo amount
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                                vmResult.PhotoAmountInTop5Tag.Add(reader.GetString(0), reader.GetInt32(1));
                        }
                    }
                    await reader.NextResultAsync();

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
