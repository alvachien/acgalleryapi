using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace acgalleryapi.ViewModels
{
    public class PhotoViewModelBase : BaseViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoId { get; set; }
        [StringLength(50)]
        public String Title { get; set; }
        [StringLength(100)]
        public String Desp { get; set; }
        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        public Int32 ThumbWidth { get; set; }
        public Int32 ThumbHeight { get; set; }
        [StringLength(100)]
        public String FileUrl { get; set; }
        [StringLength(100)]
        public String ThumbnailFileUrl { get; set; }
        public String FileFormat { get; set; }
        public DateTime UploadedTime { get; set; }
        [StringLength(50)]
        public String UploadedBy { get; set; }
        [StringLength(100)]
        public String OrgFileName { get; set; }
        public Boolean IsOrgThumbnail { get; set; }
        public Boolean IsPublic { get; set; }
    }

    public class PhotoViewModel : PhotoViewModelBase
    {
        public List<ExifTagItem> ExifTags = new List<ExifTagItem>();
        public List<String> Tags = new List<string>();        
    }

    public class PhotoViewModelEx : PhotoViewModel
    {
        // This class adds the information for FineUploader required:
        // success: success flag
        // error: error message

        public PhotoViewModelEx(Boolean bSuc, String strErr = "")
        {
            success = bSuc;
            error = strErr;
        }

        public Boolean success;
        public String error;
    }

    public class PhotoTagViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoId { get; set; }
        [Required]
        [StringLength(50)]
        public String TagString { get; set; }

    }
}
