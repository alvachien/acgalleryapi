using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace acgalleryapi.ViewModels
{
    public class AlbumViewModel
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
        public String AccessCode { get; set; }

        public Int32 PhotoCount { get; set; }
        // First photo
        public String FirstPhotoThumnailUrl { get; set; }
    }
    public class AlbumWithPhotoViewModel : AlbumViewModel
    {
        public List<PhotoViewModel> PhotoList = new List<PhotoViewModel>();
    }

    public class AlbumPhotoLinkViewModel
    {
        [Required]
        public Int32 AlbumID { get; set; }
        [Required]
        [StringLength(40)]
        public String PhotoID { get; set; }
    }

    public class AlbumPhotoByAlbumViewModel
    {
        [Required]
        public Int32 AlbumID { get; set; }
        public List<String> PhotoIDList = new List<string>();
    }

    public class AlbumPhotoByPhotoViewModel
    {
        [Required]
        [StringLength(40)]
        public String PhotoID { get; set; }
        public List<Int32> AlbumIDList = new List<Int32>();
    }
}
