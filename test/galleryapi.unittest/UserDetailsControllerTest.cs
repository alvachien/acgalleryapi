﻿using GalleryAPI.Controllers;
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
    public class UserDetailsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public UserDetailsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(DataSetupUtility.UserA)]
        [InlineData(DataSetupUtility.UserB)]
        public async Task TestCase_ReadSingle(String usrid)
        {
            var context = fixture.GetCurrentDataContext();

            this.fixture.InitTestData(context);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(
                //It.IsAny<string>(usrid)
                usrid
            );

            if (!String.IsNullOrEmpty(usrid))
            {
                var control = new UserDetailsController(context, mockHttpContextAccessor.Object);

                var getrst = control.Get(usrid);
                Assert.NotNull(getrst);
                var getokrst = Assert.IsType<OkObjectResult>(getrst);
                var usrreadobj = Assert.IsAssignableFrom<UserDetail>(getokrst.Value);
                Assert.Equal(usrid, usrreadobj.UserID);
            }

            await context.DisposeAsync();
        }
    }
}

