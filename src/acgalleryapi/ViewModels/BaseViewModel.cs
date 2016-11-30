using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace acgalleryapi.ViewModels
{
    public abstract class BaseViewModel
    {

    }

    public class BaseListViewModel<T> where T : BaseViewModel
    {
        // Runtime information
        public Int32 TotalCount { get; set; }
        public List<T> ContentList = new List<T>();

        public void Add(T tObj)
        {
            this.ContentList.Add(tObj);
        }
    }
}
