﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        // POST: api/PhotoSearch
        [HttpPost]
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
                String subqueries = filters.GetFullWhereClause();
                StringBuilder sb = new StringBuilder();

                if (usrObj == null)
                {
                    // Anonymous user
                    sb.Append(@"SELECT count(*) FROM [dbo].[View_Photo] WHERE [IsPublic] = 1 ");
                    if (String.IsNullOrEmpty(subqueries))
                    {
                        sb.Append("; ");
                    }
                    else
                    {
                        sb.Append(" AND " + subqueries + "; ");
                    }
                    sb.Append(PhotoController.GetPhotoViewSql() + @" WHERE [IsPublic] = 1");
                    if (String.IsNullOrEmpty(subqueries))
                    {
                    }
                    else
                    {
                        sb.Append(" AND " + subqueries);
                    }
                    sb.Append(@" ORDER BY (SELECT NULL) 
                            OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ");
                }
                else
                {
                    // Signed-in user
                    sb.Append(@"SELECT count(*) FROM [dbo].[View_Photo] WHERE ([IsPublic] = 1 OR [UploadedBy] = N'" + usrObj.Value + "')");
                    if (String.IsNullOrEmpty(subqueries))
                    {
                        sb.Append("; ");
                    }
                    else
                    {
                        sb.Append(" AND " + subqueries + "; ");
                    }
                    sb.Append(PhotoController.GetPhotoViewSql() + @"WHERE ([IsPublic] = 1 OR [UploadedBy] = N'" + usrObj.Value + "')");
                    if (String.IsNullOrEmpty(subqueries))
                    {
                    }
                    else
                    {
                        sb.Append(" AND " + subqueries);
                    }
                    sb.Append(@" ORDER BY (SELECT NULL) 
                            OFFSET " + skip.ToString() + " ROWS FETCH NEXT " + top.ToString() + " ROWS ONLY; ");
                }
                queryString = sb.ToString();
#if DEBUG
                System.Diagnostics.Debug.WriteLine(queryString);
#endif
                await conn.OpenAsync();

                SqlCommand cmd = new SqlCommand(queryString, conn);
                SqlDataReader reader = cmd.ExecuteReader();

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
                        PhotoController.DataRowToPhoto(reader, rst);
                        rstFiles.Add(rst);
                    }
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
                conn = null;
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
