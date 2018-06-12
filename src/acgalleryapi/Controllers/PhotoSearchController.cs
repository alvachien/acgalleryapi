using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using acgalleryapi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace acgalleryapi.Controllers
{
    [Produces("application/json")]
    [Route("api/PhotoSearch")]
    public class PhotoSearchController : Controller
    {
        // GET: api/PhotoSearch
        [HttpGet]
        public async Task<IActionResult> Get([FromBody]PhotoSearchFilterViewModel filters, [FromQuery]Int32 top = 100, Int32 skip = 0)
        {
            BaseListViewModel<PhotoViewModel> rstFiles = new BaseListViewModel<PhotoViewModel>();
            SqlConnection conn = new SqlConnection(Startup.DBConnectionString);
            Boolean bError = false;
            String strErrMsg = "";

            try
            {
                var usrObj = User.FindFirst(c => c.Type == "sub");
                String queryString = String.Empty;
                String subqueries = String.Empty;

                for (Int32 i = 0; i < filters.FieldList.Count; i++)
                {
                    subqueries += filters.FieldList[i].GenerateSql();
                    if (i != filters.FieldList.Count - 1)
                        subqueries += " AND ";
                }

                if (usrObj == null)
                {
                    // Anonymous user
                    queryString = @"SELECT count(*) FROM [dbo].[Photo] WHERE [IsPublic] = 1; " + 
                               @"; SELECT [PhotoID]
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
                          WHERE [IsPublic] = 1
                          ORDER BY (SELECT NULL) 
                            OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";
                }
                else
                {
                    // Signed-in user
                    queryString = @"SELECT count(*) FROM [dbo].[Photo] 
                          WHERE [IsPublic] = 1 OR [UploadedBy] = N'" + usrObj.Value + "'; " +
                      @"SELECT [PhotoID]
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
                          WHERE [IsPublic] = 1 OR [UploadedBy] = N'" + usrObj.Value + "' ORDER BY (SELECT NULL) OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ";
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
                            rstFiles.TotalCount = reader.GetInt32(0);
                            break;
                        }

                        if (rstFiles.TotalCount == 0)
                            break;
                    }
                    else
                    {
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

                    ++nRstBatch;

                    if (reader.NextResult())
                        continue;
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

            return new ObjectResult(rstFiles);
        }

        // GET: api/PhotoSearch/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }        
    }
}
