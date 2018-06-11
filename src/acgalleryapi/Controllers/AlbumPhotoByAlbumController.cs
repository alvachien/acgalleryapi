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
    [Produces("application/json")]
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
                        if (!usrReader.IsDBNull(0))
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

                    String queryString = @"SELECT [CreatedBy] FROM [dbo].[Album] WHERE [AlbumID] = " + vm.AlbumID.ToString();

                    SqlCommand cmd = new SqlCommand(queryString, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
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
                    else
                    {
                        return NotFound();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Delete the records from album                    
                    cmdText = @"DELETE FROM [dbo].[AlbumPhoto] WHERE [AlbumID] = " + vm.AlbumID.ToString();
                    SqlTransaction tran = conn.BeginTransaction();

                    try
                    {
                        cmd = new SqlCommand(cmdText, conn, tran);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Dispose();
                        cmd = null;

                        foreach (String pid in vm.PhotoIDList)
                        {
                            cmdText = @"INSERT INTO [dbo].[AlbumPhoto]
                               ([AlbumID]
                               ,[PhotoID])
                             VALUES(" + vm.AlbumID.ToString()
                                 + @", N'" + pid
                                 + @"')";
                            cmd = new SqlCommand(cmdText, conn, tran);
                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }

                        tran.Commit();
                    }
                    catch (Exception exp)
                    {
                        tran.Rollback();
                        throw exp;
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                return StatusCode(500, exp.Message);
            }

            return new EmptyResult();
        }
    }
}
