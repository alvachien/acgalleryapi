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
    [Produces("application/json")]
    [Route("api/albumphotobyphoto")]
    public class AlbumPhotoByPhotoController : Controller
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumPhotoByPhotoViewModel vm)
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
            // var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;
            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    UserOperatorAuthEnum? authAlbum = null;
                    String cmdText = @"SELECT [AlbumChange] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrName + "'";
                    SqlCommand cmdUserRead = new SqlCommand(cmdText, conn);
                    SqlDataReader usrReader = await cmdUserRead.ExecuteReaderAsync();
                    if (usrReader.HasRows)
                    {
                        usrReader.Read();
                        authAlbum = (UserOperatorAuthEnum)usrReader.GetByte(0);
                    }

                    if (!authAlbum.HasValue)
                    {
                        throw new Exception("User has no authoirty set yet!");
                    }
                    usrReader.Close();
                    usrReader = null;
                    cmdUserRead.Dispose();
                    cmdUserRead = null;

                    String albumList = String.Join(",", vm.AlbumIDList);

                    String queryString = @"SELECT [CreatedBy] FROM [dbo].[Album] WHERE [AlbumID] IN ( " + albumList + " )";

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        String strCreatedBy = String.Empty;
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                strCreatedBy = reader.GetString(0);

                            if (authAlbum.HasValue && authAlbum.Value == UserOperatorAuthEnum.All)
                            {
                                // Do nothing
                            }
                            else if (authAlbum.HasValue && authAlbum.Value == UserOperatorAuthEnum.OnlyOwner)
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
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        cmdText = @"DELETE FROM [dbo].[AlbumPhoto] WHERE [PhotoID] = N'" + vm.PhotoID + "'";
                        cmd = new SqlCommand(cmdText, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        foreach (Int32 aid in vm.AlbumIDList)
                        {
                            cmdText = @"INSERT INTO [dbo].[AlbumPhoto]
                               ([AlbumID]
                               ,[PhotoID]) VALUES(" + aid.ToString() + @", N'" + vm.PhotoID + @"')";
                            cmd = new SqlCommand(cmdText, conn, tran);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }

                        tran.Commit();
                    }
                    catch(Exception exp)
                    {
                        tran.Rollback();
                        throw exp;
                    }
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
