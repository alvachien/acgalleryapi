using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    public enum UserOperatorAuthEnum : Byte
    {
        All = 1,
        OnlyOwner = 2
    }

    [Table("UserDetail")]
    public class UserDetail
    {
        [Key]
        [StringLength(50)]
        public string UserID { get; set; }
        [Required]
        [StringLength(50)]
        public string DisplayAs { get; set; }
        public Int32? UploadFileMinSize { get; set; }
        public Int32? UploadFileMaxSize { get; set; }
        public Boolean? AlbumCreate { get; set; }
        public UserOperatorAuthEnum? AlbumChange { get; set; }
        public UserOperatorAuthEnum? AlbumDelete { get; set; }
        public UserOperatorAuthEnum? AlbumRead { get; set; }
        public Boolean? PhotoUpload { get; set; }
        public UserOperatorAuthEnum? PhotoChange { get; set; }
        public UserOperatorAuthEnum? PhotoDelete { get; set; }
    }
}
