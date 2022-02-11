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

            await context.DisposeAsync();
        }
    }
}
