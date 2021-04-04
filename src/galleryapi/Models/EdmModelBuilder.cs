using System;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace GalleryAPI.Models
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            var albums = builder.EntitySet<Album>("Albums");
            albums.EntityType.Ignore(emp => emp.AccessCode);
            builder.EntitySet<Photo>("Photos");
            builder.EntitySet<AlbumPhoto>("AlbumPhotos");
            builder.EntitySet<PhotoTag>("PhotoTags");
            builder.EntitySet<UserDetail>("UserDetails");
            builder.EntitySet<PhotoView>("PhotoViews");

            // Function on Album - Get Photos
            var functionWithOptional = builder.EntityType<Album>().Collection.Function("GetPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
            functionWithOptional.Parameter<int>("AlbumID");
            functionWithOptional.Parameter<string>("AccessCode").Optional();

            // Function on Album - GetAlbumPhotos
            var funcOnEntity = builder.EntityType<Album>().Function("GetRelatedPhotos").ReturnsCollectionFromEntitySet<Photo>("Photos");
            funcOnEntity.Parameter<string>("AccessCode");

            // Action on Album - Change Access Code
            var action = builder.EntityType<Album>().Action("ChangeAccessCode");
            action.Parameter<string>("AccessCode"); // .Optional();
            action.Returns<Boolean>();

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
            //    _edmModel = builder.GetEdmModel();
            //}

            return builder.GetEdmModel();
        }

    }
}
