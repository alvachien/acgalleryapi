using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GalleryAPI.Models;

namespace GalleryAPI.unittest
{
    [Collection("API_UnitTests#1")]
    public class ModelTest
    {
        [Fact]
        public void FakeTest()
        {
            // Album
            var objAlbum = new Album();
            objAlbum.AccessCode = "Test";
            objAlbum.PhotoCount = 1;
            objAlbum.Desp = "Test";
            objAlbum.AccessCodeHint = "Test"; 
            objAlbum.AlbumPhotos = new List<AlbumPhoto>();

            Assert.NotNull(objAlbum);

            // PhotoView
            var objphotoview = new PhotoView();
            objphotoview.AVNumber = "Test";
            objphotoview.CameraMaker = "Test";
            objphotoview.CameraModel = "Test";
            objphotoview.Desp = "Test";
            objphotoview.FileUrl = "Test";
            objphotoview.ThumbHeight = 100;
            objphotoview.ThumbWidth = 100;
            objphotoview.OrgFileName = "Test";
            objphotoview.IsOrgThumbnail = true;
            objphotoview.IsPublic = true;

            Assert.NotNull(objphotoview);

            // Album photo view
            var objalbumphotoview = new AlbumPhotoView();
            objalbumphotoview.AlbumID = 1;
            objalbumphotoview.AVNumber= "Test";
            objalbumphotoview.CameraMaker = "Test";
            objalbumphotoview.CameraModel = "Test";
            objalbumphotoview.Desp = "Test";
            objalbumphotoview.FileUrl = "Test";
            objalbumphotoview.ThumbHeight = 100;
            objalbumphotoview.IsPublic = true;
            objalbumphotoview.LensModel = "Test";
            Assert.NotNull(objalbumphotoview);
        }
    }
}
