using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace GalleryAPI.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            if (_edmModel == null)
            {
                var builder = new ODataConventionModelBuilder();
                var albums = builder.EntitySet<Album>("Albums");
                albums.EntityType.Ignore(emp => emp.AccessCode);
                builder.EntitySet<Photo>("Photos");

                //// two overload function import
                //var function = builder.Function("CalcByRating");
                //function.Parameter<int>("order");
                //function.ReturnsFromEntitySet<Customer>("Customers");

                //function = builder.Function("CalcByRating");
                //function.Parameter<string>("name");
                //function.ReturnsFromEntitySet<Customer>("Customers");

                //// action import
                //var action = builder.Action("CalcByRatingAction");
                //action.Parameter<int>("order");
                //action.ReturnsFromEntitySet<Customer>("Customers");

                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

    }
}
