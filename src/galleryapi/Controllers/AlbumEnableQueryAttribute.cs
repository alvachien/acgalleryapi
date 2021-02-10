using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalleryAPI.Controllers
{
    public class AlbumEnableQueryAttribute : EnableQueryAttribute
    {
        private readonly DefaultQuerySettings defaultQuerySettings;

        public AlbumEnableQueryAttribute()
        {
            this.defaultQuerySettings = new DefaultQuerySettings();
            this.defaultQuerySettings.EnableExpand = true;
            this.defaultQuerySettings.EnableSelect = true;
        }

        public override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOpts)
        {
            if (queryOpts != null && queryOpts.SelectExpand != null)
                queryOpts.SelectExpand.Validator = new AlbumExpandValidator(this.defaultQuerySettings);
            base.ValidateQuery(request, queryOpts);
        }
    }
}
