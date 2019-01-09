using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using acgalleryapi.ViewModels;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace acgalleryapi.Controllers
{
    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        private IMemoryCache _cache;

        public PhotoController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetPhotos([FromQuery] String albumid = null, [FromQuery] String accessCode = null, [FromQuery] Int32 top = 100, [FromQuery] Int32 skip = 0)
        {
            BaseListViewModel<PhotoViewModel> rstFiles = new BaseListViewModel<PhotoViewModel>();
            String strErrMsg = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                var usrObj = User.FindFirst(c => c.Type == "sub");
                String queryString = String.Empty;
                String strAlbumAC = String.Empty;
                String strCreatedBy = String.Empty;
                Boolean bIsPublic = false;
                UserOperatorAuthEnum? authRead = null;

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    if (usrObj != null)
                    {
                        String cmdText = @"SELECT [AlbumRead] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrObj.Value + "'";
                        cmd = new SqlCommand(cmdText, conn);
                        reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (!reader.IsDBNull(0))
                                authRead = (UserOperatorAuthEnum)reader.GetByte(0);
                        }

                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }

                    if (String.IsNullOrEmpty(albumid))
                    {
                        if (usrObj == null)
                        {
                            // Anonymous user
                            queryString = @"SELECT count(*) FROM [dbo].[Photo] WHERE [IsPublic] = 1; "
                                + GetPhotoViewSql()
                                + @"WHERE [IsPublic] = 1 ORDER BY (SELECT NULL) 
                                    OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";
                        }
                        else
                        {
                            // Signed-in user
                            queryString = @"SELECT count(*) FROM [dbo].[Photo] WHERE [IsPublic] = 1 OR [UploadedBy] = N'" 
                              + usrObj.Value + "'; "
                              + GetPhotoViewSql()
                              + @" WHERE [IsPublic] = 1 OR [UploadedBy] = N'"
                              + usrObj.Value + "' ORDER BY (SELECT NULL) OFFSET "
                              + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";
                        }
                    }
                    else
                    {
                        String queryString2 = @"SELECT [AlbumID]
                                  ,[CreatedBy]
                                  ,[IsPublic]
                                  ,[AccessCode]
                              FROM [dbo].[Album]
                              WHERE [AlbumID] = " + albumid.ToString();

                        cmd = new SqlCommand(queryString2, conn);
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            reader.Read(); // Only one record!

                            if (!reader.IsDBNull(1))
                                strCreatedBy = reader.GetString(1);
                            if (!reader.IsDBNull(2))
                                bIsPublic = reader.GetBoolean(2);
                            if (!reader.IsDBNull(3))
                                strAlbumAC = reader.GetString(3);
                        }
                        reader.Dispose();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;

                        if (usrObj == null)
                        {
                            // Anonymous user
                            if (!bIsPublic)
                            {
                                errorCode = HttpStatusCode.Unauthorized;
                                throw new Exception();
                            }

                            if (!String.IsNullOrEmpty(strAlbumAC))
                            {
                                if (String.IsNullOrEmpty(accessCode))
                                {
                                    errorCode = HttpStatusCode.Unauthorized;
                                    throw new Exception();
                                }
                                else
                                {
                                    if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                    {
                                        errorCode = HttpStatusCode.Unauthorized;
                                        throw new Exception();
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Signed-in user
                            if (authRead.HasValue && authRead.Value == UserOperatorAuthEnum.OnlyOwner)
                            {
                                if (String.CompareOrdinal(strCreatedBy, usrObj.Value) != 0)
                                {
                                    // Not the album creator then needs the access code
                                    if (bIsPublic)
                                    {
                                        if (!String.IsNullOrEmpty(strAlbumAC))
                                        {
                                            if (String.IsNullOrEmpty(accessCode))
                                            {
                                                errorCode = HttpStatusCode.Unauthorized;
                                                throw new Exception();
                                            }
                                            else
                                            {
                                                if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                                {
                                                    errorCode = HttpStatusCode.Unauthorized;
                                                    throw new Exception();
                                                }
                                                else
                                                {
                                                    // Access code accepted, do nothing
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Non public album, current user has no authority to view it.
                                        errorCode = HttpStatusCode.Unauthorized;
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    // Creator of album, no need to access code at all
                                }
                            }
                            else if (authRead.HasValue && authRead.Value == UserOperatorAuthEnum.All)
                            {
                                // Do nothing~
                            }
                            else
                            {
                                // Though logged in, but without any rights, it is the same as unlogged in user
                                if (!bIsPublic)
                                {
                                    errorCode = HttpStatusCode.Unauthorized;
                                    throw new Exception();
                                }
                                if (!String.IsNullOrEmpty(strAlbumAC))
                                {
                                    if (String.IsNullOrEmpty(accessCode))
                                    {
                                        errorCode = HttpStatusCode.Unauthorized;
                                        throw new Exception();
                                    }
                                    else
                                    {
                                        if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                        {
                                            errorCode = HttpStatusCode.Unauthorized;
                                            throw new Exception();
                                        }
                                    }
                                }
                            }
                        }

                        queryString = @"SELECT count(*) FROM [dbo].[AlbumPhoto] AS taba
                                LEFT OUTER JOIN [dbo].[Photo] AS tabb
                                    ON taba.[PhotoID] = tabb.[PhotoID]
                            WHERE taba.[AlbumID] = N'" + albumid + "'; " +
                                @"SELECT tabb.[PhotoID]
                              ,tabb.[Title]
                              ,tabb.[Desp]
                              ,tabb.[Width]
                              ,tabb.[Height]
                              ,tabb.[ThumbWidth]
                              ,tabb.[ThumbHeight]
                              ,tabb.[UploadedAt]
                              ,tabb.[UploadedBy]
                              ,tabb.[OrgFileName]
                              ,tabb.[PhotoUrl]
                              ,tabb.[PhotoThumbUrl]
                              ,tabb.[IsOrgThumb]
                              ,tabb.[ThumbCreatedBy]
                              ,tabb.[CameraMaker]
                              ,tabb.[CameraModel]
                              ,tabb.[LensModel]
                              ,tabb.[AVNumber]
                              ,tabb.[ShutterSpeed]
                              ,tabb.[ISONumber]
                              ,tabb.[IsPublic]
                              ,tabb.[EXIFInfo] 
                              ,tabb.[Rating]
                              ,tabb.[Tags]
                            FROM [dbo].[AlbumPhoto] AS taba
                                LEFT OUTER JOIN [dbo].[View_Photo] AS tabb
                                    ON taba.[PhotoID] = tabb.[PhotoID]
                            WHERE taba.[AlbumID] = N'" + albumid + "' ORDER BY (SELECT NULL) OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";
                    }

                    cmd = new SqlCommand(queryString, conn);
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            rstFiles.TotalCount = reader.GetInt32(0);
                            break;
                        }
                    }
                    reader.NextResult();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            PhotoViewModel rst = new PhotoViewModel();

                            DataRowToPhoto(reader, rst);

                            rstFiles.Add(rst);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
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
                        return StatusCode(500, strErrMsg);
                }
            }

            return new ObjectResult(rstFiles);
        }

        // GET api/photo/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Forbid();
        }

        // POST api/photo
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]PhotoViewModel vm)
        {
            if (vm == null)
                return BadRequest("No data is inputted");

            if (vm.Title != null)
                vm.Title = vm.Title.Trim();
            if (String.IsNullOrEmpty(vm.Title))
                return BadRequest("Title is a must!");

            // Update the database
            SqlTransaction tran = null;
            String strErrMsg = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                // ID is set to identity
                String queryString = @"INSERT INTO [dbo].[Photo]
                           ([PhotoID]
                           ,[Title]
                           ,[Desp]
                           ,[Width]
                           ,[Height]
                           ,[ThumbWidth]
                           ,[ThumbHeight]
                           ,[UploadedAt]
                           ,[UploadedBy]
                           ,[OrgFileName]
                           ,[PhotoUrl]
                           ,[PhotoThumbUrl]
                           ,[IsOrgThumb]
                           ,[ThumbCreatedBy]
                           ,[CameraMaker]
                           ,[CameraModel]
                           ,[LensModel]
                           ,[AVNumber]
                           ,[ShutterSpeed]
                           ,[ISONumber]
                           ,[IsPublic]
                           ,[EXIFInfo])
                     VALUES
                           (@PhotoID
                           ,@Title
                           ,@Desp
                           ,@Width
                           ,@Height
                           ,@ThumbWidth
                           ,@ThumbHeight
                           ,@UploadedAt
                           ,@UploadedBy
                           ,@OrgFileName
                           ,@PhotoUrl
                           ,@PhotoThumbUrl
                           ,@IsOrgThumb
                           ,@ThumbCreatedBy
                           ,@CameraMaker
                           ,@CameraModel
                           ,@LensModel
                           ,@AVNumber
                           ,@ShutterSpeed
                           ,@ISONumber
                           ,@IsPublic
                           ,@EXIF)";

                string strMaker = string.Empty, strModel = string.Empty, strLens = string.Empty, strAV = string.Empty, strSpeed = string.Empty, strISO = string.Empty;
                if (vm.ExifTags.Count > 0)
                {
                    vm.ExifTags.RemoveAll(eti =>
                    {
                        return eti.group != "EXIF" && eti.group != "Composite" && eti.group != "XMP";
                    });

                    foreach (var et in vm.ExifTags)
                    {
                        if (et.name == "Camera Model Name")
                        {
                            if (String.IsNullOrEmpty(strModel))
                                strModel = et.value;
                        }
                        else if (et.name == "Make")
                        {
                            if (String.IsNullOrEmpty(strMaker))
                                strMaker = et.value;
                        }
                        else if (et.name == "Exposure Time" || et.name == "Shutter Speed Value")
                        {
                            if (String.IsNullOrEmpty(strSpeed))
                                strSpeed = et.value;
                        }
                        else if (et.name == "F Number" || et.name == "Aperture Value")
                        {
                            if (String.IsNullOrEmpty(strAV))
                                strAV = et.value;
                        }
                        else if (et.name == "ISO")
                        {
                            if (String.IsNullOrEmpty(strISO))
                                strISO = et.value;
                        }
                        else if (et.name == "Lens Info" || et.name == "Lens")
                        {
                            if (String.IsNullOrEmpty(strLens))
                                strLens = et.value;
                        }
                    }
                }

                using(conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();
                    tran = conn.BeginTransaction();

                    // Create photo entry
                    cmd = new SqlCommand(queryString, conn, tran);
                    cmd.Parameters.AddWithValue("@PhotoID", vm.PhotoId);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", vm.Desp);
                    cmd.Parameters.AddWithValue("@Width", vm.Width);
                    cmd.Parameters.AddWithValue("@Height", vm.Height);
                    cmd.Parameters.AddWithValue("@ThumbWidth", vm.ThumbWidth);
                    cmd.Parameters.AddWithValue("@ThumbHeight", vm.ThumbHeight);
                    cmd.Parameters.AddWithValue("@UploadedAt", vm.UploadedTime);
                    cmd.Parameters.AddWithValue("@UploadedBy", vm.UploadedBy);
                    cmd.Parameters.AddWithValue("@OrgFileName", vm.OrgFileName);
                    cmd.Parameters.AddWithValue("@PhotoUrl", vm.FileUrl);
                    cmd.Parameters.AddWithValue("@PhotoThumbUrl", vm.ThumbnailFileUrl);
                    cmd.Parameters.AddWithValue("@IsOrgThumb", vm.IsOrgThumbnail);
                    cmd.Parameters.AddWithValue("@ThumbCreatedBy", 2); // 1 for ExifTool, 2 stands for others

                    cmd.Parameters.AddWithValue("@CameraMaker", String.IsNullOrEmpty(strMaker) ? DBNull.Value : (object)strMaker);
                    cmd.Parameters.AddWithValue("@CameraModel", String.IsNullOrEmpty(strModel) ? DBNull.Value : (object)strModel);
                    cmd.Parameters.AddWithValue("@LensModel", String.IsNullOrEmpty(strLens) ? DBNull.Value : (object)strLens);
                    cmd.Parameters.AddWithValue("@AVNumber", String.IsNullOrEmpty(strAV) ? DBNull.Value : (object)strAV);
                    cmd.Parameters.AddWithValue("@ShutterSpeed", String.IsNullOrEmpty(strSpeed) ? DBNull.Value : (object)strSpeed);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    cmd.Parameters.AddWithValue("@ISONumber", 0);

                    String strJson = Newtonsoft.Json.JsonConvert.SerializeObject(vm.ExifTags);
                    cmd.Parameters.AddWithValue("@EXIF", strJson);

                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    // Create the tags
                    if (vm.Tags.Count > 0)
                    {
                        foreach (var tag in vm.Tags)
                        {
                            queryString = @"INSERT INTO [dbo].[PhotoTag] ([PhotoID],[Tag]) VALUES (@PhotoID, @Tag)";
                            cmd = new SqlCommand(queryString, conn, tran);
                            cmd.Parameters.AddWithValue("@PhotoID", vm.PhotoId);
                            cmd.Parameters.AddWithValue("@Tag", tag);

                            await cmd.ExecuteNonQueryAsync();
                            cmd.Dispose();
                            cmd = null;
                        }
                    }

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();

#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
            }
            finally
            {
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
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

            return new ObjectResult(vm);
        }

        // PUT api/photo/5
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody]PhotoViewModel vm)
        {
            if (vm == null)
                return BadRequest("No data is inputted");

            if (vm.Title != null)
                vm.Title = vm.Title.Trim();
            if (String.IsNullOrEmpty(vm.Title))
                return BadRequest("Title is a must!");

            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj == null || String.IsNullOrEmpty(usrObj.Value))
                return BadRequest("User info cannot load");

            String strErrMsg = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                String cmdText = @"UPDATE [Photo]
                               SET [Title] = @Title
                                  ,[Desp] = @Desp
                             WHERE [PhotoID] = @PhotoID";

                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    UserOperatorAuthEnum? authChange = null;
                    if (usrObj != null)
                    {
                        String sqlString = @"SELECT [PhotoChange] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrObj.Value + "'";
                        cmd = new SqlCommand(sqlString, conn);
                        reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (!reader.IsDBNull(0))
                                authChange = (UserOperatorAuthEnum)reader.GetByte(0);
                        }

                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }
                    if (authChange == null)
                    {
                        errorCode = HttpStatusCode.Unauthorized;
                        throw new Exception();
                    }
                    else
                    {
                        if (authChange.Value == UserOperatorAuthEnum.OnlyOwner)
                        {
                            String sqlString = @"SELECT [UploadedBy] FROM [dbo].[Photo] WHERE [PhotoID] = N'" + usrObj.Value + "'";
                            cmd = new SqlCommand(sqlString, conn);
                            reader = await cmd.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                reader.Read();

                                if (!reader.IsDBNull(0))
                                {
                                    if (String.CompareOrdinal(reader.GetString(0), usrObj.Value) != 0)
                                    {
                                        errorCode = HttpStatusCode.Unauthorized;
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    // Not exist
                                    errorCode = HttpStatusCode.NotFound;
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                errorCode = HttpStatusCode.NotFound;
                                throw new Exception();
                            }

                            reader.Close();
                            reader = null;
                            cmd.Dispose();
                            cmd = null;
                        }
                    }

                    tran = conn.BeginTransaction();

                    // Update the photo itself
                    cmd = new SqlCommand(cmdText, conn, tran);
                    cmd.Parameters.AddWithValue("@PhotoID", vm.PhotoId);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", vm.Desp);

                    await cmd.ExecuteNonQueryAsync();

                    // Update the rating

                    // Update the tags

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
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

            return new ObjectResult(vm);
        }

        // DELETE api/photo/5
        [HttpDelete("{pid}")]
        [Authorize]
        public async Task<IActionResult> Delete(string pid)
        {
            if (String.IsNullOrEmpty(pid))
                return BadRequest("No data is inputted");

            var usrObj = User.FindFirst(c => c.Type == "sub");
            if (usrObj == null || String.IsNullOrEmpty(usrObj.Value))
                return BadRequest("User info cannot load");

            UserOperatorAuthEnum? authDelete = null;
            String strErrMsg = "";
            SqlConnection conn = null;
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            SqlTransaction tran = null;
            HttpStatusCode errorCode = HttpStatusCode.OK;

            try
            {
                String sqlString = string.Empty;
                using (conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    if (usrObj != null)
                    {
                        sqlString = @"SELECT [PhotoDelete] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrObj.Value + "'";
                        cmd = new SqlCommand(sqlString, conn);
                        reader = await cmd.ExecuteReaderAsync();
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (!reader.IsDBNull(0))
                                authDelete = (UserOperatorAuthEnum)reader.GetByte(0);
                        }

                        reader.Close();
                        reader = null;
                        cmd.Dispose();
                        cmd = null;
                    }
                    if (authDelete == null)
                    {
                        errorCode = HttpStatusCode.Unauthorized;
                        throw new Exception();
                    }
                    else
                    {
                        if (authDelete.Value == UserOperatorAuthEnum.OnlyOwner)
                        {
                            sqlString = @"SELECT [UploadedBy] FROM [dbo].[Photo] WHERE [PhotoID] = N'" + usrObj.Value + "'";
                            cmd = new SqlCommand(sqlString, conn);
                            reader = await cmd.ExecuteReaderAsync();
                            if (reader.HasRows)
                            {
                                reader.Read();

                                if (!reader.IsDBNull(0))
                                {
                                    if (String.CompareOrdinal(reader.GetString(0), usrObj.Value) != 0)
                                    {
                                        errorCode = HttpStatusCode.Unauthorized;
                                        throw new Exception();
                                    }
                                }
                                else
                                {
                                    // Not exist
                                    errorCode = HttpStatusCode.NotFound;
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                errorCode = HttpStatusCode.NotFound;
                                throw new Exception();
                            }

                            reader.Close();
                            reader= null;
                            cmd.Dispose();
                            cmd = null;
                        }
                    }

                    tran = conn.BeginTransaction();

                    sqlString = @"DELETE [Photo] WHERE [PhotoID] = @PhotoID";
                    cmd = new SqlCommand(sqlString, conn, tran);
                    cmd.Parameters.AddWithValue("@PhotoID", pid);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    sqlString = @"DELETE FROM [dbo].[PhotoTag] WHERE [PhotoID] = @PhotoID";
                    cmd = new SqlCommand(sqlString, conn, tran);
                    cmd.Parameters.AddWithValue("@PhotoID", pid);
                    await cmd.ExecuteNonQueryAsync();
                    cmd.Dispose();
                    cmd = null;

                    tran.Commit();
                }
            }
            catch (Exception exp)
            {
                if (tran != null)
                    tran.Rollback();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(exp.Message);
#endif
                strErrMsg = exp.Message;
                if (errorCode == HttpStatusCode.OK)
                    errorCode = HttpStatusCode.InternalServerError;
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

        // Get photo view SQL
        internal static string GetPhotoViewSql()
        {
            return @"SELECT [PhotoID]
                    ,[Title]
                    ,[Desp]
                    ,[Width]
                    ,[Height]
                    ,[ThumbWidth]
                    ,[ThumbHeight]
                    ,[UploadedAt]
                    ,[UploadedBy]
                    ,[OrgFileName]
                    ,[PhotoUrl]
                    ,[PhotoThumbUrl]
                    ,[IsOrgThumb]
                    ,[ThumbCreatedBy]
                    ,[CameraMaker]
                    ,[CameraModel]
                    ,[LensModel]
                    ,[AVNumber]
                    ,[ShutterSpeed]
                    ,[ISONumber]
                    ,[IsPublic]
                    ,[EXIFInfo]
                    ,[Rating]
                    ,[Tags]
                    FROM [dbo].[View_Photo]";
        }

        // Row to ViewModel
        internal static void DataRowToPhoto(SqlDataReader reader, PhotoViewModel rst)
        {
            Int32 idx = 0;
            rst.PhotoId = reader.GetString(idx++);
            rst.Title = reader.GetString(idx++);
            rst.Desp = reader.GetString(idx++);
            if (!reader.IsDBNull(idx))
                rst.Width = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.Height = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.ThumbWidth = reader.GetInt32(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.ThumbHeight = reader.GetInt32(idx++);
            else
                ++idx;
            rst.UploadedTime = reader.GetDateTime(idx++);
            if (!reader.IsDBNull(idx))
                rst.UploadedBy = reader.GetString(idx++);
            else
                ++idx;
            rst.OrgFileName = reader.GetString(idx++);
            rst.FileUrl = reader.GetString(idx++); // 11 - 1
            if (!reader.IsDBNull(idx))
                rst.ThumbnailFileUrl = reader.GetString(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.IsOrgThumbnail = reader.GetBoolean(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@ThumbCreatedBy", 2); // 1 for ExifTool, 2 stands for others; // 14
            ++idx;
            //cmd.Parameters.AddWithValue("@CameraMaker", "To-do"); // 15
            if (!reader.IsDBNull(idx))
                rst.CameraMaker = reader.GetString(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@CameraModel", "To-do"); // 16
            if (!reader.IsDBNull(idx))
                rst.CameraModel = reader.GetString(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@LensModel", "To-do");   // 17
            if (!reader.IsDBNull(idx))
                rst.LensModel = reader.GetString(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@AVNumber", "To-do");    // 18
            if (!reader.IsDBNull(idx))
                rst.AVNumber = reader.GetString(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@ShutterSpeed", "To-do"); // 19
            if (!reader.IsDBNull(idx))
                rst.ShutterSpeed = reader.GetString(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@ISONumber", 0);         // 20
            if (!reader.IsDBNull(idx))
                rst.ISONumber = reader.GetInt32(idx++);
            else
                ++idx;
            //cmd.Parameters.AddWithValue("@IsPublic", true);       // 21
            if (!reader.IsDBNull(idx))
                rst.IsPublic = reader.GetBoolean(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.ExifTags = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExifTagItem>>(reader.GetString(idx++));
            else
                ++idx;
            if (!reader.IsDBNull(idx))
                rst.AvgRating = reader.GetDouble(idx++);
            else
                ++idx;
            if (!reader.IsDBNull(idx))
            {
                rst.Tags.AddRange(reader.GetString(idx).Split(','));
            }
        }
    }
}
