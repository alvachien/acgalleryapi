using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    [Table("AlbumPhoto")]
    public class AlbumPhoto
    {
        [Key]
        [Column("AlbumID", TypeName = "INT")]
        public Int32 AlbumID { get; set; }

        public Album CurrentAlbum { get; set; }

        [Key]
        [StringLength(40)]
        [Column("PhotoID", TypeName = "NVARCHAR(40)")]
        public String PhotoID { get; set; }

        public Photo CurrentPhoto { get; set; }
    }
}
