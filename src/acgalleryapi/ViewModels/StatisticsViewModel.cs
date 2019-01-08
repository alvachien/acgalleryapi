using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace acgalleryapi.ViewModels
{
    public sealed class StatisticsViewModel
    {
        public StatisticsViewModel()
        {
            this.PhotoAmountInTop5Album = new List<int>();
            this.PhotoAmountInTop5Tag = new Dictionary<string, int>();
        }

        public Int32 PhotoAmount { get; set; }
        public Int32 AlbumAmount { get; set; }
        public List<Int32> PhotoAmountInTop5Album { get; set; }
        public Dictionary<String, Int32> PhotoAmountInTop5Tag { get; set; }
    }
}
