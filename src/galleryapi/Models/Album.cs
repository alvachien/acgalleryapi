using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    [Table("Album")]
    public class Album
    {
        [Key]
        [Column("AlbumID", TypeName = "INT")]
        public Int32 Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Title", TypeName = "NVARCHAR(50)")]
        public String Title { get; set; }

        [StringLength(100)]
        [Column("Desp", TypeName = "NVARCHAR(100)")]
        public String Desp { get; set; }

        [StringLength(50)]
        [Column("CreatedBy", TypeName = "NVARCHAR(50)")]
        public String CreatedBy { get; set; }

        [Column("CreateAt")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [Column("IsPublic", TypeName = "BIT")]
        public Boolean IsPublic { get; set; }

        [StringLength(50)]
        [Column("AccessCodeHint", TypeName = "NVARCHAR(50)")]
        public String AccessCodeHint { get; set; }

        /**
         * The field AccessCodeRequired only valid in list view mode
         * In the list view mode, it shall never return the real access code,
         * meaning, the AccessCode is empty;
         */
        //public Boolean AccessCodeRequired { get; set; }

        [StringLength(50)]
        [Column("AccessCode", TypeName = "NVARCHAR(50)")]
        public String AccessCode { get; set; }

        //public Int32 PhotoCount { get; set; }
        //// First photo
        //public String FirstPhotoThumnailUrl { get; set; }
    }
}
