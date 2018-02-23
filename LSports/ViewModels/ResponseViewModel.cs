using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.ViewModels
{
    public class ResponseViewModel<T> 
    {
        public int Status { get; set; }
        public IList<Error> Errors { get; set; }
        public T Data{get;set;}
    }
}
