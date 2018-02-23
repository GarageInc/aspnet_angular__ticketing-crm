using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models;

namespace LSports.Framework.Models.CustomClasses
{
    public class TicketFieldModel
    {
        public int Id { get; set; }
        public int? TicketId { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public int? CustomFieldId { get; set; }
        public string TextValue { get; set; }
        public CustomField CustomField { get; set; }
        public IList<DropdownItem> OriginalValues { get; set; }
    }
}
