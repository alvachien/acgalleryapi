using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryAPI.Models;
using Microsoft.OData;

namespace GalleryAPI.Controllers
{
    public class AlbumExpandValidator : SelectExpandQueryValidator
    {
        public AlbumExpandValidator(DefaultQuerySettings defaultQuerySettings)
            : base(defaultQuerySettings)
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
