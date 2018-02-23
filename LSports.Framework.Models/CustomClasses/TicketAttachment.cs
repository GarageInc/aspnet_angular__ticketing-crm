
namespace LSports.Framework.Models.CustomClasses
{
    public class TicketAttachment
    {
        public int TicketFieldId { get; set; }
        public int FileId { get; set; }
        public int CustomFieldId { get; set; }
        public string FileName { get; set; }
        public string CustomFieldTitle { get; set; }
    }
}
