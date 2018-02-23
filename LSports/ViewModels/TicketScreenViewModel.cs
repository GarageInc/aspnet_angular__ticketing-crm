using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;

namespace LSports.ViewModels
{
    public class TicketScreenViewModel
    {
        public TicketModel Ticket { get; set; }
        public IList<ControlEnum> Controls { get; set; }
        public TicketLog Log { get; set; }
        public IList<CustomFieldValue> CustomFieldValues { get; set; } 
        public string InstructionsTextHtml { get; set; }
        public string InstructionsLinksHtml { get; set; }
        public IList<TicketAttachment> Attchments { get; set; }
        public bool ShowCustomerDetails { get; set; }
        public bool IsTicketClosed { get; set; }
    }
}
