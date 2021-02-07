using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    [Table("Photo")]
    public class Photo
    {
        [Key]
        [StringLength(40)]
        [Column("PhotoID", TypeName = "NVARCHAR(40)")]
        public String PhotoId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Title", TypeName = "NVARCHAR(50)")]
        public String Title { get; set; }

        [StringLength(100)]
        [Column("Desp", TypeName = "NVARCHAR(100)")]
        public String Desp { get; set; }

        [Column("Width", TypeName = "INT")]
        public Int32 Width { get; set; }

        [Column("Height", TypeName = "INT")]
        public Int32 Height { get; set; }

        [Column("ThumbWidth", TypeName = "INT")]
        public Int32 ThumbWidth { get; set; }

        [Column("ThumbHeight", TypeName = "INT")]
        public Int32 ThumbHeight { get; set; }

        [StringLength(100)]
        [Column("PhotoUrl", TypeName = "NVARCHAR(100)")]
        public String FileUrl { get; set; }

        [StringLength(100)]
        [Column("PhotoThumbUrl", TypeName = "NVARCHAR(100)")]
        public String ThumbnailFileUrl { get; set; }

        //public String FileFormat { get; set; }

        [Column("UploadedAt")]
        [DataType(DataType.Date)]
        public DateTime UploadedTime { get; set; }

        [StringLength(50)]
        [Column("UploadedBy", TypeName = "NVARCHAR(50)")]
        public String UploadedBy { get; set; }

        [StringLength(100)]
        [Column("OrgFileName", TypeName = "NVARCHAR(100)")]
        public String OrgFileName { get; set; }

        [Column("IsOrgThumb", TypeName = "BIT")]
        public Boolean IsOrgThumbnail { get; set; }

        [Column("IsPublic", TypeName = "BIT")]
        public Boolean IsPublic { get; set; }

        [StringLength(50)]
        [Column("CameraMaker", TypeName = "NVARCHAR(50)")]
        public String CameraMaker { get; set; }

        [StringLength(100)]
        [Column("CameraModel", TypeName = "NVARCHAR(100)")]
        public String CameraModel { get; set; }

        [StringLength(100)]
        [Column("LensModel", TypeName = "NVARCHAR(100)")]
        public string LensModel { get; set; }

        [StringLength(20)]
        [Column("AVNumber", TypeName = "NVARCHAR(20)")]
        public String AVNumber { get; set; }

        [StringLength(50)]
        [Column("ShutterSpeed", TypeName = "NVARCHAR(50)")]
        public string ShutterSpeed { get; set; }

        [Column("ISONumber", TypeName = "INT")]
        public Int32 ISONumber { get; set; }
    }
}
