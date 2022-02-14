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
    public class AlbumsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public AlbumsControllerTest(SqliteDatabaseFixture fixture)
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

            var control = new AlbumsController(context, mockHttpContextAccessor.Object);

            var getrst = control.Get();
            Assert.NotNull(getrst);
            var getokrst = Assert.IsType<OkObjectResult>(getrst);
            var allalbums = Assert.IsAssignableFrom<IQueryable<Album>>(getokrst.Value);

            if (String.IsNullOrEmpty(usrid))
            {
                Assert.Equal(1, allalbums.Count());
            }
            else if (usrid == DataSetupUtility.UserA)
            {
                Assert.Equal(2, allalbums.Count());
            }
            else if (usrid == DataSetupUtility.UserB)
            {
                Assert.Equal(2, allalbums.Count());
            }

            await context.DisposeAsync();
        }

        [Theory]
        [InlineData(null, 1)]
        [InlineData(DataSetupUtility.UserA, 1)]
        [InlineData(DataSetupUtility.UserB, 1)]
        public async Task TestCase_ReadSingle(String usrid, int albumid)
        {
            var context = fixture.GetCurrentDataContext();

            this.fixture.InitTestData(context);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(
                //It.IsAny<string>(usrid)
                usrid
            );

            var control = new AlbumsController(context, mockHttpContextAccessor.Object);

            var getrst = control.Get(albumid);
            Assert.NotNull(getrst);
            //var getokrst = Assert.IsType<OkObjectResult>(getrst);
            //var allalbums = Assert.IsAssignableFrom<IQueryable<Album>>(getokrst.Value);

            //if (String.IsNullOrEmpty(usrid))
            //{
            //    Assert.Equal(1, allalbums.Count());
            //}
            //else if (usrid == DataSetupUtility.UserA)
            //{
            //    Assert.Equal(2, allalbums.Count());
            //}
            //else if (usrid == DataSetupUtility.UserB)
            //{
            //    Assert.Equal(2, allalbums.Count());
            //}

            await context.DisposeAsync();
        }
    }
}
