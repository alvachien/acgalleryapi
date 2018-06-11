using System;
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
            SqlConnection conn = null;
            String queryString = "";
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
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
                        SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCodeHint, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
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
                        queryString = @"SELECT 0;

                            WITH albumfirstphoto as (
	                            SELECT tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                            INNER JOIN dbo.Photo as tabc
	                                ON tabb.PhotoID = tabc.PhotoID
	                                GROUP BY tabb.AlbumID)
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCodeHint, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
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
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCodeHint, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
	                            tabb.PhotoCount, tabb.ThumbUrl
	                        FROM dbo.Album as taba
	                        LEFT OUTER JOIN albumfirstphoto as tabb
		                        on taba.AlbumID = tabb.AlbumID
                            WHERE taba.IsPublic = 1 or (taba.IsPublic = 0 and taba.CreatedBy = N'" + usrObj.Value + @"')
                            ORDER BY (SELECT NULL)
                            OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; "; ;
                    }
                    else
                    {
                        queryString = @"SELECT 0;

                            WITH albumfirstphoto AS (
	                        SELECT tabb.AlbumID, count(tabb.PhotoID) as PhotoCount, min(tabc.PhotoThumbUrl) as ThumbUrl from dbo.AlbumPhoto as tabb
	                        JOIN dbo.Photo as tabc
	                        ON tabb.PhotoID = tabc.PhotoID
	                        GROUP BY tabb.AlbumID)
                            SELECT taba.AlbumID, taba.Title, taba.Desp, taba.IsPublic, taba.AccessCodeHint, taba.AccessCode, taba.CreateAt, taba.CreatedBy,
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

                conn = new SqlConnection(Startup.DBConnectionString);
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
                            Int32 idx = 0;
                            avm.Id = reader.GetInt32(idx++);
                            avm.Title = reader.GetString(idx++);
                            if (!reader.IsDBNull(idx))
                                avm.Desp = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                avm.IsPublic = reader.GetBoolean(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                avm.AccessCodeHint = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                            {
                                if (!String.IsNullOrEmpty(reader.GetString(idx)))
                                    avm.AccessCodeRequired = true;
                                else
                                    avm.AccessCodeRequired = false;
                                ++idx;
                            }
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                avm.CreatedAt = reader.GetDateTime(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                avm.CreatedBy = reader.GetString(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                                avm.PhotoCount = (Int32)reader.GetInt32(idx++);
                            else
                                ++idx;
                            if (!reader.IsDBNull(idx))
                            {
                                avm.FirstPhotoThumnailUrl = reader.GetString(idx++);

                                if (avm.AccessCodeRequired)
                                    avm.FirstPhotoThumnailUrl = String.Empty;
                            }
                            else
                                ++idx;
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
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                    conn = null;
                }
            }

            if (bError)
                return StatusCode(500, strErrMsg);

            return new ObjectResult(listVm);
        }

        // GET api/album/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            SqlConnection conn = null;
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
                          ,[AccessCodeHint]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + id.ToString();

                conn = new SqlConnection(Startup.DBConnectionString);
                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read(); // Only one record!

                    avm = new AlbumViewModel();
                    Int32 idx = 0;
                    avm.Id = reader.GetInt32(idx++);
                    avm.Title = reader.GetString(idx++);
                    if (!reader.IsDBNull(idx))
                        avm.Desp = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        avm.CreatedBy = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        avm.CreatedAt = reader.GetDateTime(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        avm.IsPublic = reader.GetBoolean(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                        avm.AccessCodeHint = reader.GetString(idx++);
                    else
                        ++idx;
                    if (!reader.IsDBNull(idx))
                    {
                        if (!String.IsNullOrEmpty(reader.GetString(idx++)))
                        {
                            avm.AccessCodeRequired = true;
                        }
                        else
                        {
                            avm.AccessCodeRequired = false;
                        }
                    }
                    else
                        ++idx;

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
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                    conn = null;
                }
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
                    await conn.OpenAsync();

                    // Step 1. Read out user authority
                    Boolean? cancreate = null;
                    String cmdText = @"SELECT [AlbumCreate] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrName + "'";
                    SqlCommand cmdUserRead = new SqlCommand(cmdText, conn);
                    SqlDataReader usrReader = await cmdUserRead.ExecuteReaderAsync();
                    if (usrReader.HasRows)
                    {
                        usrReader.Read();
                        if (!usrReader.IsDBNull(0))
                            cancreate = usrReader.GetBoolean(0);
                    }

                    if (!cancreate.HasValue || cancreate.Value == false)
                    {
                        throw new Exception("User has no authoirty set yet!");
                    }

                    usrReader.Close();
                    usrReader = null;
                    cmdUserRead.Dispose();
                    cmdUserRead = null;

                    // Step 2. Create an album
                    cmdText = @"INSERT INTO [dbo].[Album]
                               ([Title]
                               ,[Desp]
                               ,[CreatedBy]
                               ,[CreateAt]
                               ,[IsPublic]
                               ,[AccessCodeHint]
                               ,[AccessCode])
                         VALUES
                               (@Title
                               ,@Desp
                               ,@CreatedBy
                               ,@CreatedAt
                               ,@IsPublic
                               ,@AccessCodeHint
                               ,@AccessCode
                                )
                         ; SELECT @Identity = SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", String.IsNullOrEmpty(vm.Desp) ? String.Empty : vm.Desp);
                    cmd.Parameters.AddWithValue("@CreatedBy", usrName);
                    cmd.Parameters.AddWithValue("@CreatedAt", vm.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                    cmd.Parameters.AddWithValue("@AccessCodeHint", String.IsNullOrEmpty(vm.AccessCodeHint) ? String.Empty : vm.AccessCodeHint);
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

                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Step 1. Read out user authority
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

                    // Step 2. Read album out
                    cmdText = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCodeHint]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + vm.Id.ToString() + " FOR UPDATE ";

                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
                        if (!reader.IsDBNull(3))
                            strCreatedBy = reader.GetString(3);

                        if (authAlbum.Value == UserOperatorAuthEnum.All)
                        {
                            // Do nothing
                        }
                        else if (authAlbum.Value == UserOperatorAuthEnum.OnlyOwner)
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

                    // Step 3. Real update
                    cmdText = @"UPDATE [Album]
                            SET [Title] = @Title
                                ,[Desp] = @Desp
                                ,[IsPublic] = @IsPublic
                                ,[AccessCodeHint] = @AccessCodeHint
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
                    if (String.IsNullOrEmpty(vm.AccessCodeHint))
                        cmd.Parameters.AddWithValue("@AccessCodeHint", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@AccessCodeHint", vm.AccessCodeHint);
                    if (String.IsNullOrEmpty(vm.AccessCode))
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

                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    await conn.OpenAsync();

                    // Step 1. Read out user authority
                    UserOperatorAuthEnum? authAlbumDelete = null;
                    String cmdText = @"SELECT [AlbumDelete] FROM [dbo].[UserDetail] WHERE [UserID] = N'" + usrName + "'";
                    SqlCommand cmdUserRead = new SqlCommand(cmdText, conn);
                    SqlDataReader usrReader = await cmdUserRead.ExecuteReaderAsync();
                    if (usrReader.HasRows)
                    {
                        usrReader.Read();
                        if (!usrReader.IsDBNull(0))
                            authAlbumDelete = (UserOperatorAuthEnum)usrReader.GetByte(0);
                    }
                    
                    if (!authAlbumDelete.HasValue)
                    {
                        throw new Exception("User has no authoirty set yet!");
                    }

                    usrReader.Close();
                    usrReader = null;
                    cmdUserRead.Dispose();
                    cmdUserRead = null;

                    // Step 2. Read out album info and check authority
                    cmdText = @"SELECT [AlbumID]
                          ,[Title]
                          ,[Desp]
                          ,[CreatedBy]
                          ,[CreateAt]
                          ,[IsPublic]
                          ,[AccessCodeHint]
                          ,[AccessCode]
                      FROM [dbo].[Album]
                      WHERE [AlbumID] = " + nID.ToString() + " FOR UPDATE ";

                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        String strCreatedBy = String.Empty;
                        if (!reader.IsDBNull(3))
                            strCreatedBy = reader.GetString(3);

                        if (authAlbumDelete.Value == UserOperatorAuthEnum.All)
                        {
                            // Do nothing
                        }
                        else if (authAlbumDelete.Value == UserOperatorAuthEnum.OnlyOwner)
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

                    // Step 3. Perform the real deletion
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

            return new OkResult();
        }
    }    
}
