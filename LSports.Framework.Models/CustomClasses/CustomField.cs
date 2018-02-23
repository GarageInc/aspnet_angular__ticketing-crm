using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models;

namespace LSports.Framework.Models.CustomClasses
{
    public class CustomField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Identifier { get; set; }
        public string PlaceholderText { get; set; }
        public int CustomFieldTypeId { get; set; }
        public string DbTableName { get; set; }
        public string DbTableIdFieldName { get; set; }
        public string DbTableTextFieldName { get; set; }
        public string DbFilterFieldName { get; set; }
        public int? StepNumber { get; set; }
        public int? RootCustomFieldId { get; set; }
        public int? DropdownCustomFieldId { get; set; }

        public CustomFieldType CustomFieldType { get; set; }
    }

    public class CustomFieldViewModel
    {
        public CustomField CustomField { get; set; }
        public int SortOrder { get; set; }
    }
}
