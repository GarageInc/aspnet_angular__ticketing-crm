using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class CustomFieldCollection : CustomField
    {
        public IList<CustomField> ItemsToAdd { get; set; }
        public IList<CustomField> ItemsToDelete { get; set; }
        public IList<CustomField> ItemsToUpdate { get; set; }
    }
}
