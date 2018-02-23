using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class RootMenuItem : MenuItem
    {
        public IList<MenuItem> Subitems { get; set; }
    }
}
