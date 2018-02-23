
namespace LSports.Framework.Models.CustomClasses
{
    public class TicketWithLastLogRecord
    {
        public TicketModel TicketModel { get; set; }
        public TicketLog LastLog { get; set; }
    }
}
