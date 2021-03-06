﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using acgalleryapi.ViewModels;
using System.Net;

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
                return BadRequest("No data is inputted");

            if (!TryValidateModel(vm))
                return BadRequest();

            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;
            String strErrMsg = String.Empty;

            var usrName = User.FindFirst(c => c.Type == "sub").Value;
            // var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;
            try
            {
                String cmdText = @"SELECT [AlbumChange] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrName + "'";
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    UserOperatorAuthEnum? authAlbum = null;
                    
                    cmd = new SqlCommand(cmdText, conn);
                    reader = await cmd.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (!reader.IsDBNull(0))
                            authAlbum = (UserOperatorAuthEnum)reader.GetByte(0);
                    }

                    if (!authAlbum.HasValue)
                    {
                        errorCode = HttpStatusCode.BadRequest;
                        throw new Exception("User has no authoirty set yet!");
                    }

                    reader.Close();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    String albumList = String.Join(",", vm.AlbumIDList);

                    String queryString = @"SELECT [CreatedBy] FROM [dbo].[Album] WHERE [AlbumID] IN ( " + albumList + " )";

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

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
                                    errorCode = HttpStatusCode.Unauthorized;
                                    throw new Exception();
                                }
                                else
                                {
                                    // Do nothing
                                }
                            }
                            else
                            {
                                errorCode = HttpStatusCode.BadRequest;
                                throw new Exception();
                            }
                        }
                    }
                    else
                    {
                        errorCode = HttpStatusCode.NotFound;
                        throw new Exception();
                    }

                    reader.Dispose();
                    reader = null;
                    cmd.Dispose();
                    cmd = null;

                    // Delete the records from album
                    tran = conn.BeginTransaction();
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
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                if (tran != null)
                    tran.Rollback();
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
                strErrMsg = exp.Message;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
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
                        return StatusCode(500, strErrMsg);
                }
            }

            return new EmptyResult();
        }
    }
}
