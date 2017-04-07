using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using acgalleryapi.ViewModels;

namespace acgalleryapi.Controllers
{
    [Route("api/albumphotobyalbum")]
    public class AlbumPhotoByAlbumController : Controller
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumPhotoByAlbumViewModel vm)
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

            // Create it into DB
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
                        reader.Read();

                        String strCreatedBy = String.Empty;
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
                    else
                    {
                        return NotFound();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    List<String> listCmds = new List<string>();
                    // Delete the records from album                    
                    String cmdText = @"DELETE FROM [dbo].[AlbumPhoto] WHERE [AlbumID] = " + vm.AlbumID.ToString();
                    listCmds.Add(cmdText);

                    foreach (String pid in vm.PhotoIDList)
                    {
                        cmdText = @"INSERT INTO [dbo].[AlbumPhoto]
                               ([AlbumID]
                               ,[PhotoID])
                             VALUES(" + vm.AlbumID.ToString()
                             + @", N'" + pid
                             + @"')";
                        listCmds.Add(cmdText);
                    }
                    String allQueries = String.Join(";", listCmds);

                    cmd = new SqlCommand(allQueries, conn);
                    await cmd.ExecuteNonQueryAsync();
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
