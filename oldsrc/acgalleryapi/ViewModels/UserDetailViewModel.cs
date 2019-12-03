using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace acgalleryapi.ViewModels
{
    public enum UserOperatorAuthEnum : Byte
    {
        All         = 1,
        OnlyOwner   = 2
    }

    public sealed class UserDetailViewModel
    {
        [Required]
        [MaxLength(50)]
        public string UserID { get; set; }
        [Required]
        [MaxLength(50)]
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
