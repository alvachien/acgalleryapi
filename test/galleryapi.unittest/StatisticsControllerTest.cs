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
    public class StatisticsControllerTest
    {
        private SqliteDatabaseFixture fixture = null;
        public StatisticsControllerTest(SqliteDatabaseFixture fixture)
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

            await context.DisposeAsync();
        }
    }
}
