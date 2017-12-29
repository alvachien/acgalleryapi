﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using acgalleryapi.ViewModels;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AlbumController : Controller
    {
        // GET: api/album
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] String photoid = null, [FromQuery] Int32 top = 100, [FromQuery] Int32 skip = 0)
        {
            BaseListViewModel<AlbumViewModel> listVm = new BaseListViewModel<AlbumViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif
                var usrObj = User.FindFirst(c => c.Type == "sub");

                if (usrObj == null)
                {
                    // Anonymous user
                    if (String.IsNullOrEmpty(photoid))
                    {
                        queryString = @"WITH albumfirstphoto as (SELECT tabb.AlbumID, COUNT(tabb.PhotoID) as PhotoCount, MIN(tabc.PhotoThumbUrl) as ThumbUrl 
                            FROM dbo.AlbumPhoto as tabb
	                        JOIN dbo.Photo as tabc
	                            ON tabb.PhotoID = tabc.PhotoID
	                            GROUP BY tabb.AlbumID)
                        SELECT COUNT(*) FROM dbo.Album as taba
	                    LEFT OUTER JOIN albumfirstphoto as tabb
		                    ON taba.AlbumID = tabb.AlbumID
                        WHERE taba.IsPublic = 1;

                        WITH albumfirstphoto as (SELECT tabb.AlbumID, COUNT(tabb.PhotoID) as PhotoCount, MIN(tabc.PhotoThumbUrl) as ThumbUrl 
                            FROM dbo.AlbumPhoto as tabb
	                        JOIN dbo.Photo as tabc
	                            ON tabb.PhotoID = tabc.PhotoID
	                            GROUP BY tabb.AlbumID)
                        SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                        tabb.PhotoCount, tabb.ThumbUrl
	                    FROM dbo.Album as taba
	                    LEFT OUTER JOIN albumfirstphoto as tabb
		                    ON taba.AlbumID = tabb.AlbumID
                        WHERE taba.IsPublic = 1
                        ORDER BY (SELECT NULL)
                        OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY;";
                    }
                    else
                    {
                        // In case the photo id is specified, won't care about the top and skip
                        queryString = @"
                            SELECT 0;

                            WITH albumfirstphoto as (
	                            SELECT tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                            INNER JOIN dbo.Photo as tabc
	                                ON tabb.PhotoID = tabc.PhotoID
	                                GROUP BY tabb.AlbumID)
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        FROM dbo.AlbumPhoto as tabc
	                        INNER JOIN dbo.Album as taba
		                        ON tabc.AlbumID = taba.AlbumID
                                AND taba.IsPublic = 1
	                        LEFT OUTER JOIN albumfirstphoto as tabb
		                        ON taba.AlbumID = tabb.AlbumID
                            WHERE tabc.PhotoID = N'";
                        queryString += photoid;
                        queryString += @"'";
                    }
                }
                else
                {
                    // Signed in user
                    if (String.IsNullOrEmpty(photoid))
                    {
                        queryString = @"WITH albumfirstphoto as (select tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl 
                            FROM dbo.AlbumPhoto as tabb
                            JOIN dbo.Photo as tabc
                                 ON tabb.PhotoID = tabc.PhotoID GROUP BY tabb.AlbumID)
                            SELECT  count(*)
                                FROM dbo.Album as taba
                            LEFT OUTER JOIN albumfirstphoto as tabb
                                ON taba.AlbumID = tabb.AlbumID
                            WHERE taba.IsPublic = 1 OR (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + "'); "
                            + 
                            @"WITH albumfirstphoto as (SELECT tabb.AlbumID, COUNT(tabb.PhotoID) as PhotoCount, MIN(tabc.PhotoThumbUrl) as ThumbUrl 
                                FROM dbo.AlbumPhoto as tabb 
                                JOIN dbo.Photo as tabc
	                                ON tabb.PhotoID = tabc.PhotoID
	                                GROUP BY tabb.AlbumID)
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        FROM dbo.Album as taba
	                        LEFT OUTER JOIN albumfirstphoto as tabb
		                        on taba.AlbumID = tabb.AlbumID
                            WHERE taba.IsPublic = 1 or (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + @"')
                            ORDER BY (SELECT NULL)
                            OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";;
                    }
                    else
                    {
                        queryString = @"
                            SELECT 0;
                            WITH albumfirstphoto AS (
	                        SELECT tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                        JOIN dbo.Photo as tabc
	                        ON tabb.PhotoID = tabc.PhotoID
	                        GROUP BY tabb.AlbumID)
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        FROM dbo.AlbumPhoto as tabc
	                        INNER JOIN dbo.Album as taba
		                        ON tabc.AlbumID = taba.AlbumID
                                AND taba.IsPublic = 1 OR (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + "') " 
                                + 
                                @" 
                            LEFT OUTER JOIN albumfirstphoto as tabb
		                      ON taba.AlbumID = tabb.AlbumID
                            WHERE tabc.PhotoID = N'";
                        queryString += photoid;
                        queryString += @"'";
                    }
                }

                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                Int32 nRstBatch = 0;
                while (reader.HasRows)
                {
                    if (nRstBatch == 0)
                    {
                        while (reader.Read())
                        {
                            listVm.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            AlbumViewModel avm = new AlbumViewModel();
                            avm.Id = reader.GetInt32(0);
                            avm.Title = reader.GetString(1);
                            if (!reader.IsDBNull(2))
                                avm.Desp = reader.GetString(2);
                            if (!reader.IsDBNull(3))
                                avm.IsPublic = reader.GetBoolean(3);
                            if (!reader.IsDBNull(4))
                            {
                                // Cannot just release the AccessCode
                                //avm.AccessCode = reader.GetString(4);
                                if (!String.IsNullOrEmpty(reader.GetString(4)))
                                    avm.AccessCode = "1";
                            }
                            if (!reader.IsDBNull(5))
                                avm.CreatedAt = reader.GetDateTime(5);
                            if (!reader.IsDBNull(6))
                                avm.CreatedBy = reader.GetString(6);
                            if (!reader.IsDBNull(7))
                                avm.PhotoCount = (Int32)reader.GetInt32(7);
                            if (!reader.IsDBNull(8))
                            {
                                avm.FirstPhotoThumnailUrl = reader.GetString(8);

                                if (!String.IsNullOrEmpty(avm.AccessCode))
                                    avm.FirstPhotoThumnailUrl = String.Empty;
                            }
                            listVm.Add(avm);
                        }

                        if (!String.IsNullOrEmpty(photoid))
                        {
                            // Need update the total count
                            listVm.TotalCount = listVm.ContentList.Count;
                        }
                    }

                    ++nRstBatch;

                    reader.NextResult();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(listVm);
        }

        // GET api/album/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            AlbumViewModel avm = null;

            var usrObj = User.FindFirst(c => c.Type == "sub");
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";
            Boolean bNotExist = false;

            try
            {
                queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + id.ToString();

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); // Only one record!

                    String strAlbumAC = String.Empty;
                    String strCreatedBy = String.Empty;
                    Boolean bIsPublic = false;
                    if (!reader.IsDBNull(3))
                        strCreatedBy = reader.GetString(3);
                    if (!reader.IsDBNull(5))
                        bIsPublic = reader.GetBoolean(5);
                    if (!reader.IsDBNull(6))
                        strAlbumAC = reader.GetString(6);

                    avm = new AlbumViewModel();
                    avm.Id = reader.GetInt32(0);
                    avm.Title = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        avm.Desp = reader.GetString(2);
                    avm.CreatedBy = strCreatedBy;
                    if (!reader.IsDBNull(4))
                        avm.CreatedAt = reader.GetDateTime(4);
                    avm.IsPublic = bIsPublic;
                    avm.AccessCode = String.IsNullOrEmpty(strAlbumAC)? String.Empty : "1";

                    reader.Dispose();
                    cmd.Dispose();
                    reader = null;
                    cmd = null;
                } 
                else
                {
                    bNotExist = true;
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            if (bNotExist)
                return NotFound();

            if (bError || avm == null)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(avm);
        }

        // POST api/album
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody]AlbumViewModel vm)
        {
            Int32 nNewID = -1;
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

            // Create it into DB
            Boolean bError = false;
            String strErrMsg = "";
            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    String cmdText = @"INSERT INTO [dbo].[Album]
                               ([Title]
                               ,[Desp]
                               ,[CreatedBy]
                               ,[CreateAt]
                               ,[IsPublic]
                               ,[AccessCode])
                         VALUES
                               (@Title
                               ,@Desp
                               ,@CreatedBy
                               ,@CreatedAt
                               ,@IsPublic
                               ,@AccessCode
                                )
                         ; SELECT @Identity = SCOPE_IDENTITY();";
                    await conn.OpenAsync();

                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", String.IsNullOrEmpty(vm.Desp) ? String.Empty : vm.Desp);
                    cmd.Parameters.AddWithValue("@CreatedBy", usrName);
                    cmd.Parameters.AddWithValue("@CreatedAt", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    cmd.Parameters.AddWithValue("@AccessCode", String.IsNullOrEmpty(vm.AccessCode) ? String.Empty : vm.AccessCode);
                    SqlParameter idparam = cmd.Parameters.AddWithValue("@Identity", SqlDbType.Int);
                    idparam.Direction = ParameterDirection.Output;

                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                    nNewID = (Int32)idparam.Value;
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            vm.Id = nNewID;
            return new ObjectResult(vm);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] AlbumViewModel vm)
        {
            if (vm == null)
            {
                return BadRequest("No data is inputted");
            }

            if (vm.Title != null)
                vm.Title = vm.Title.Trim();
            if (String.IsNullOrEmpty(vm.Title))
            {
                return BadRequest("Title is a must!");
            }

            Boolean bError = false;
            String strErrMsg = "";
            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumChange").Value;

                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    String cmdText = String.Empty;

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + vm.Id.ToString() + " FOR UPDATE ";

                    conn.Open();
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

                    cmd.Dispose();
                    cmd = null;

                    cmdText = @"UPDATE [Album]
                            SET [Title] = @Title
                                ,[Desp] = @Desp
                                ,[IsPublic] = @IsPublic
                                ,[AccessCode] = @AccessCode
                            WHERE [AlbumID] = @Id
                        ";
                    cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@Id", vm.Id);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    if (String.IsNullOrEmpty(vm.Desp))
                        cmd.Parameters.AddWithValue("@Desp", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Desp", vm.Desp);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    if (vm.AccessCode == null)
                        cmd.Parameters.AddWithValue("@AccessCode", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@AccessCode", vm.AccessCode);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
            }
            finally
            {
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(vm);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(Int32 nID)
        {
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrName = User.FindFirst(c => c.Type == "sub").Value;
                var scopeStr = User.FindFirst(c => c.Type == "GalleryAlbumDelete").Value;

                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    String cmdText = String.Empty;

                    String queryString = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + nID.ToString() + " FOR UPDATE ";

                    conn.Open();
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

                    cmd.Dispose();
                    cmd = null;

                    cmdText = @"DELETE FROM [Album] WHERE [AlbumID] = " + nID.ToString();
                    cmd = new SqlCommand(cmdText, conn);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
                strErrMsg = exp.Message;
                bError = true;
            }
            finally
            {
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new EmptyResult();
        }
    }
    
}
