using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

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
                var functionWithOptional = albums.EntityType.Collection.Function("GetPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
                functionWithOptional.Parameter<int>("AlbumID");
                functionWithOptional.Parameter<string>("AccessCode").Optional();

                // Function on Album - GetAlbumPhotos
                // NOT WORKING!!!
                //var funcOnEntity = builder.EntityType<Album>().Function("GetAlbumPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
                //funcOnEntity.Parameter<string>("AccessCode").Optional();

                // Function on Album - Change Access Code
                var action = builder.EntityType<Album>().Collection.Action("ChangeAccessCode").ReturnsFromEntitySet<Album>("Albums");
                action.Parameter<int>("AlbumID");
                action.Parameter<string>("AccessCode").Optional();
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

                //builder.EnableLowerCamelCase();
                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

    }
}
