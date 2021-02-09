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
                builder.EntitySet<AlbumPhoto>("AlbumPhotos");
                builder.EntitySet<PhotoTag>("PhotoTags");
                builder.EntitySet<UserDetail>("UserDetails");

                // Function on Album - Get Photos
                var functionWithOptional = albums.EntityType.Collection.Function("GetPhotos");
                functionWithOptional.ReturnsCollectionFromEntitySet<Photo>("Photos");
                functionWithOptional.Parameter<int>("AlbumID");
                functionWithOptional.Parameter<int>("AccessCode").Optional();
                // functionWithOptional.Parameter<string>("aveSalary").HasDefaultValue("129");

                //var function = albums.EntityType.Collection.Function("GetPhotos");
                //function.Parameter<int>("AlbumID");
                //function.Parameter<string>("AccessCode");
                //function.ReturnsCollectionFromEntitySet<Photo>("Photos");
                var function = albums.EntityType.Collection.Function("GetPhotos2");
                function.ReturnsCollectionFromEntitySet<Photo>("Photos");
                function = albums.EntityType.Function("GetPhotos3");
                function.ReturnsCollectionFromEntitySet<Photo>("Photos");

                // Function on Album - Change Access Code
                var action = albums.EntityType.Action("ChangeAccessCode");
                action.Parameter<int>("AlbumID");
                action.Parameter<string>("AccessCode");
                action.Returns<int>();

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
