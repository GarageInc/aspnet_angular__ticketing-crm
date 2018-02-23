
using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public int? Count { get; set; }
        public object Parameters { get; set; } 
    }
}
