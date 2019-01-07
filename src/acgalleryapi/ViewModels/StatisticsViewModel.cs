using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace acgalleryapi.ViewModels
{
    public sealed class StatisticsViewModel
    {
        public Int32 PhotoAmount { get; set; }
        public Int32 AlbumAmount { get; set; }
        public Int32 PhotoAmountInTop5Album { get; set; }
    }
}
