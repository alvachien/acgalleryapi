using GalleryAPI.Controllers;
using GalleryAPI.Models;
using GalleryAPI.test.common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GalleryAPI.unittest
{
    [Collection("API_UnitTests#1")]
    public class PhotosControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public PhotosControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.UserB)]
        public async Task TestCase_ReadList(String usrid)
        {
            var context = fixture.GetCurrentDataContext();

            this.fixture.InitTestData(context);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(
                //It.IsAny<string>(usrid)
                usrid
            );

            var control = new PhotosController(context, mockHttpContextAccessor.Object);
            var rst = control.Get();
            Assert.NotNull(rst);

            await context.DisposeAsync();
        }
    }
}
