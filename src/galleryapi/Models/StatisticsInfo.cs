using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    public class StatisticsInfo
    {
        public StatisticsInfo()
        {
            this.PhotoAmountInTop5Album = new Dictionary<int, int>();
            this.PhotoAmountInTop5Tag = new Dictionary<string, int>();
        }

        public Int32 PhotoAmount { get; set; }
        public Int32 AlbumAmount { get; set; }
        public Dictionary<int, int> PhotoAmountInTop5Album { get; set; }
        public Dictionary<String, Int32> PhotoAmountInTop5Tag { get; set; }
    }
}
