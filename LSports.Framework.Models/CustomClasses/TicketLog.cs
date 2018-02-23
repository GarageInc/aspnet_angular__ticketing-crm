using System;

namespace LSports.Framework.Models.CustomClasses
{
    public class TicketLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int? ActorId { get; set; }
        public int EntryTypeId { get; set; }
        public string EntryValue { get; set; }
        public string EntryExtendedValue { get; set; }

        public virtual TicketModel TicketModel { get; set; }
        public DateTime? LogDate { get; set; }
    }
}
