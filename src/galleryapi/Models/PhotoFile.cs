using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Models
{
    /// <summary>
    /// Structure used for uploaded image.
    /// Based on : https://github.com/blueimp/jQuery-File-Upload/wiki/Setup
    /// </summary>
    
    public class PhotoFileResult
    {
        public string name { get; set; }
        public int size { get; set; }
    }

    public sealed class PhotoFileSuccess : PhotoFileResult
    {
        public int width { get; set; }
        public int height { get; set; }
        public int thumbwidth { get; set; }
        public int thumbheight { get; set; }
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string deleteUrl { get; set; }
        public string deleteType { get; set; }
    }

    public sealed class PhotoFileError : PhotoFileResult
    {
        public string error { get; set; }
    }

    public sealed class PhotoFileSuccessResult
    {
        public List<PhotoFileSuccess> files { get; set; }
    }

    public sealed class PhotoFileErrorResult
    {
        public List<PhotoFileError> files { get; set; }
    }

    // For deletion, need dynamic json~
    //{"files": [
    //  {
    //    "picture1.jpg": true
    //  },
    //  {
    //    "picture2.jpg": true
    //  }
    //]}
}
