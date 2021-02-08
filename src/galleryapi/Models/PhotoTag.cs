using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    [Table("PhotoTag")]
    public class PhotoTag
    {
        [Key]
        [StringLength(40)]
        [Column("PhotoID", TypeName = "NVARCHAR(40)")]
        public String PhotoID { get; set; }

        [Key]
        [StringLength(50)]
        [Column("Tag", TypeName = "NVARCHAR(50)")]
        public String TagString { get; set; }

        public Photo CurrentPhoto { get; set; }
    }
}
