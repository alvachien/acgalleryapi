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
    [Route("api/[controller]")]
    public class PhotoController : Controller
    {
        [HttpGet]
        public IActionResult GetPhotos([FromQuery] String albumid = null, String accessCode = null)
        {
            List<PhotoViewModel> rstFiles = new List<PhotoViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);

            try
            {
                conn.Open();

#if DEBUG
                foreach (var clm in User.Claims.AsEnumerable())
                {
                    System.Diagnostics.Debug.WriteLine("Type = " + clm.Type + "; Value = " + clm.Value);
                }
#endif

                var usrObj = User.FindFirst(c => c.Type == "sub");
                String queryString = String.Empty;
                String strAlbumAC = String.Empty;
                String strCreatedBy = String.Empty;
                Boolean bIsPublic = false;

                if (String.IsNullOrEmpty(albumid))
                {
                    if (usrObj == null)
                    {
                        // Anonymous user
                        queryString = @"SELECT [PhotoID]
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
                          FROM [dbo].[Photo] 
                          WHERE [IsPublic] = 1";
                    }
                    else
                    {
                        // Signed-in user
                        var usrName = User.FindFirst(c => c.Type == "sub").Value;
                        queryString = @"SELECT [PhotoID]
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
                          FROM [dbo].[Photo] 
                          WHERE [IsPublic] = 1 OR [UploadedBy] = " + usrName;
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

                    SqlCommand cmd2 = new SqlCommand(queryString2, conn);
                    SqlDataReader reader2 = cmd2.ExecuteReader();

                    if (reader2.HasRows)
                    {
                        reader2.Read(); // Only one record!

                        if (!reader2.IsDBNull(1))
                            strCreatedBy = reader2.GetString(1);
                        if (!reader2.IsDBNull(2))
                            bIsPublic = reader2.GetBoolean(2);
                        if (!reader2.IsDBNull(3))
                            strAlbumAC = reader2.GetString(3);
                    }
                    reader2.Dispose();
                    reader2 = null;
                    cmd2.Dispose();
                    cmd2 = null;

                    if (usrObj == null)
                    {
                        // Anonymous user
                        if (!bIsPublic)
                        {
                            return Unauthorized();
                        }

                        if (!String.IsNullOrEmpty(strAlbumAC))
                        {
                            if (String.IsNullOrEmpty(accessCode))
                            {
                                return Unauthorized();
                            }
                            else
                            {
                                if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                {
                                    return Unauthorized();
                                }
                            }
                        }
                    }
                    else
                    {
                        // Signed-in user
                        var scopeStr = User.FindFirst(c => c.Type == "GalleryNonPublicAlbumRead").Value;
                        var usrName = User.FindFirst(c => c.Type == "sub").Value;

                        if (String.CompareOrdinal(scopeStr, "OnlyOwner") == 0)
                        {
                            if (String.CompareOrdinal(strCreatedBy, usrName) != 0)
                            {
                                // Not the album creator then needs the access code
                                if (bIsPublic)
                                {
                                    if (!String.IsNullOrEmpty(strAlbumAC))
                                    {
                                        if (String.IsNullOrEmpty(accessCode))
                                        {
                                            return Unauthorized();
                                        }
                                        else
                                        {
                                            if (String.CompareOrdinal(strAlbumAC, accessCode) != 0)
                                            {
                                                return Unauthorized();
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
                                    return Unauthorized();
                                }
                            }
                            else
                            {
                                // Creator of album, no need to access code at all
                            }
                        }
                        else if (String.CompareOrdinal(scopeStr, "All") == 0)
                        {
                            // Do nothing~
                        }
                        else
                        {
                            // Shall never happened!
                            return BadRequest();
                        }
                    }

                    queryString = @"SELECT tabb.[PhotoID]
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
                              ,tabb.[EXIFInfo] 
                            FROM [dbo].[AlbumPhoto] AS taba
                                LEFT OUTER JOIN [dbo].[Photo] AS tabb
                                    ON taba.[PhotoID] = tabb.[PhotoID]
                            WHERE taba.[AlbumID] = " + albumid;
                }

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PhotoViewModel rst = new PhotoViewModel();

                    //cmd.Parameters.AddWithValue("@PhotoID", nid.ToString("N"));   // 1
                    rst.PhotoId = reader.GetString(0);
                    //cmd.Parameters.AddWithValue("@Title", nid.ToString("N"));     // 2
                    rst.Title = reader.GetString(1);
                    //cmd.Parameters.AddWithValue("@Desp", nid.ToString("N"));      // 3
                    rst.Desp = reader.GetString(2);
                    if (!reader.IsDBNull(3))
                        rst.Width = reader.GetInt32(3);
                    if (!reader.IsDBNull(4))
                        rst.Height = reader.GetInt32(4);
                    if (!reader.IsDBNull(5))
                        rst.ThumbWidth = reader.GetInt32(5);
                    if (!reader.IsDBNull(6))
                        rst.ThumbHeight = reader.GetInt32(6);
                    //cmd.Parameters.AddWithValue("@UploadedAt", DateTime.Now);     // 8
                    rst.UploadedTime = reader.GetDateTime(7);
                    //cmd.Parameters.AddWithValue("@UploadedBy", "Tester");         // 9
                    //cmd.Parameters.AddWithValue("@OrgFileName", rst.OrgFileName); // 10
                    rst.OrgFileName = reader.GetString(9);
                    //cmd.Parameters.AddWithValue("@PhotoUrl", rst.FileUrl);        // 11
                    rst.FileUrl = reader.GetString(10); // 11 - 1
                    //cmd.Parameters.AddWithValue("@PhotoThumbUrl", rst.ThumbnailFileUrl); // 12
                    if (!reader.IsDBNull(11)) // 12 - 1
                        rst.ThumbnailFileUrl = reader.GetString(11);
                    //cmd.Parameters.AddWithValue("@IsOrgThumb", bThumbnailCreated);    // 13
                    //cmd.Parameters.AddWithValue("@ThumbCreatedBy", 2); // 1 for ExifTool, 2 stands for others; // 14
                    //cmd.Parameters.AddWithValue("@CameraMaker", "To-do"); // 15
                    //cmd.Parameters.AddWithValue("@CameraModel", "To-do"); // 16
                    //cmd.Parameters.AddWithValue("@LensModel", "To-do");   // 17
                    //cmd.Parameters.AddWithValue("@AVNumber", "To-do");    // 18
                    //cmd.Parameters.AddWithValue("@ShutterSpeed", "To-do"); // 19
                    //cmd.Parameters.AddWithValue("@ISONumber", 0);         // 20
                    //cmd.Parameters.AddWithValue("@IsPublic", true);       // 21
                    if (!reader.IsDBNull(20))
                        rst.IsPublic = reader.GetBoolean(20);
                    //String strJson = Newtonsoft.Json.JsonConvert.SerializeObject(rst.ExifTags);
                    //cmd.Parameters.AddWithValue("@EXIF", strJson);        // 22
                    if (!reader.IsDBNull(21))
                        rst.ExifTags = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ExifTagItem>>(reader.GetString(21));

                    rstFiles.Add(rst);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return new ObjectResult(rstFiles);
        }

        // GET api/photo/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/photo
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody]PhotoViewModel vm)
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

            // Update the database
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);

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

                conn.Open();
                SqlCommand cmd = new SqlCommand(queryString, conn);
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
                cmd.Parameters.AddWithValue("@CameraMaker", "To-do");
                cmd.Parameters.AddWithValue("@CameraModel", "To-do");
                cmd.Parameters.AddWithValue("@LensModel", "To-do");
                cmd.Parameters.AddWithValue("@AVNumber", "To-do");
                cmd.Parameters.AddWithValue("@ShutterSpeed", "To-do");
                cmd.Parameters.AddWithValue("@IsPublic", vm.IsPublic);
                cmd.Parameters.AddWithValue("@ISONumber", 0);

                String strJson = Newtonsoft.Json.JsonConvert.SerializeObject(vm.ExifTags);
                cmd.Parameters.AddWithValue("@EXIF", strJson);

                try
                {
                    Int32 nRst = await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception exp)
                {
                    System.Diagnostics.Debug.WriteLine(exp.Message);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return new ObjectResult(vm);
        }

        // PUT api/photo/5
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody]PhotoViewModel vm)
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

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    String cmdText = @"UPDATE [Photo]
                               SET [Title] = @Title
                                  ,[Desp] = @Desp
                             WHERE [PhotoID] = @PhotoID
                            ";

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@PhotoID", vm.PhotoId);
                    cmd.Parameters.AddWithValue("@Title", vm.Title);
                    cmd.Parameters.AddWithValue("@Desp", vm.Desp);

                    await cmd.ExecuteNonQueryAsync();
                    return new ObjectResult(true);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
            }

            return new ObjectResult(false);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string pid)
        {
            if (String.IsNullOrEmpty(pid))
            {
                return BadRequest("No data is inputted");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Startup.DBConnectionString))
                {
                    String cmdText = @"DELETE [Photo]
                             WHERE [PhotoID] = @PhotoID
                            ";

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(cmdText, conn);
                    cmd.Parameters.AddWithValue("@PhotoID", pid);

                    await cmd.ExecuteNonQueryAsync();
                    return new ObjectResult(true);
                }
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            finally
            {
            }

            return new ObjectResult(false);
        }
    }
}
