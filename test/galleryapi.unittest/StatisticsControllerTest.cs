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
    public class StatisticsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public StatisticsControllerTest(SqliteDatabaseFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestCase_Read()
        {
            var context = fixture.GetCurrentDataContext();

            this.fixture.InitTestData(context);

            var control = new StatisticsController(context);
            var getrst = control.Get();
            Assert.NotNull(getrst);
            var getokrst = Assert.IsType<ObjectResult>(getrst);
            var statisinfo = Assert.IsType<StatisticsInfo>(getokrst.Value);

            await context.DisposeAsync();
        }
    }
}
