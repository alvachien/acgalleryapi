using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryAPI.Models;
using Microsoft.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.ModelBuilder.Config;

namespace GalleryAPI.Controllers
{
    public class AlbumExpandValidator : SelectExpandQueryValidator
    {
        public AlbumExpandValidator() : base()
        {
        }

        public override void Validate(SelectExpandQueryOption selectExpandQueryOption,
            ODataValidationSettings validationSettings)
        {
            if (selectExpandQueryOption.RawExpand.Contains(nameof(AlbumPhoto)))
            {
                throw new ODataException(
                    $"Expand on Album for {nameof(AlbumPhoto)} not allowed");
            }

            base.Validate(selectExpandQueryOption, validationSettings);
        }
    }
}
