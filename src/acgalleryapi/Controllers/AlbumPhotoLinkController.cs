using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using acgalleryapi.ViewModels;

namespace acgalleryapi.Controllers
{
    [Route("api/albumphotolink")]
    public class AlbumPhotoLinkController : Controller
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumPhotoLinkViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (TryValidateModel(vm))
            {
            }
            else
            {
                return BadRequest();
            }

            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;
            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + vm.AlbumID.ToString();

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        String strCreatedBy = String.Empty;
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(3))
                                strCreatedBy = reader.GetString(3);

                            if (String.CompareOrdinal(scopeStr, "All") == 0)
                            {
                                // Do nothing
                            }
                            else if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                            {
                                if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                                {
                                    return Unauthorized();
                                }
                                else
                                {
                                    // Do nothing
                                }
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                    }
                    else
                    {
                        return NotFound();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Delete the records from album
                    String cmdText = @"INSERT INTO [dbo].[AlbumPhoto] ([AlbumID], [PhotoID]) VALUES( " + vm.AlbumID.ToString() + ", N'" + vm.PhotoID + "');";
                    cmd = new SqlCommand(cmdText, conn);
                    await cmd.ExecuteNonQueryAsync();

                    cmd.Dispose();
                    cmd = null;
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                return StatusCode(500, exp.Message);
            }

            return new EmptyResult();
        }
    }
}
