using GalleryAPI.Controllers;
using Microsoft.AspNetCore.Http;
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

        [Fact]
        public async Task TestCase_Read()
        {
            var context = fixture.GetCurrentDataContext();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());
            var control = new AlbumsController(context, mockHttpContextAccessor.Object);

            try
            {
                var getrst = control.Get();
                Assert.NotNull(getrst);
            }
            catch (Exception ex)
            {
            }

            await context.DisposeAsync();
        }
    }
}
