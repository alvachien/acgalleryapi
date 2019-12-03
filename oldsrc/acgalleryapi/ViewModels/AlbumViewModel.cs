using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace acgalleryapi.ViewModels
{
    public class AlbumViewModel : BaseViewModel
    {
        public Int32 Id { get; set; }
        [Required]
        [StringLength(50)]
        public String Title { get; set; }
        [StringLength(100)]
        public String Desp { get; set; }
        [StringLength(50)]
        public String CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Boolean IsPublic { get; set; }
        [StringLength(50)]
        public String AccessCodeHint { get; set; }
        /**
         * The field AccessCodeRequired only valid in list view mode
         * In the list view mode, it shall never return the real access code,
         * meaning, the AccessCode is empty;
         */
        public Boolean AccessCodeRequired { get; set; }
        [StringLength(50)]
        public String AccessCode { get; set; }

        public Int32 PhotoCount { get; set; }
        // First photo
        public String FirstPhotoThumnailUrl { get; set; }
    }
    public class AlbumWithPhotoViewModel : AlbumViewModel
    {
        public List<PhotoViewModel> PhotoList = new List<PhotoViewModel>();
    }

    public class AlbumPhotoLinkViewModel : BaseViewModel
    {
        [Required]
        public Int32 AlbumID { get; set; }
        [Required]
        [StringLength(40)]
        public String PhotoID { get; set; }
    }

    public class AlbumPhotoByAlbumViewModel : BaseViewModel
    {
        [Required]
        public Int32 AlbumID { get; set; }
        public List<String> PhotoIDList = new List<string>();
    }

    public class AlbumPhotoByPhotoViewModel : BaseViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoID { get; set; }
        public List<Int32> AlbumIDList = new List<Int32>();
    }
}
