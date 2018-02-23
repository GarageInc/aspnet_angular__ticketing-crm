//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LSports.Framework.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class Ticket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Ticket()
        {
            this.TicketsLogs = new HashSet<TicketsLog>();
        }
    
        public int Id { get; set; }
        public System.DateTime CreationDate { get; set; }
        public System.DateTime LastUpdate { get; set; }
        public int StatusId { get; set; }
        public int CustomerId { get; set; }
        public int CustomerUserId { get; set; }
        public int PriorityId { get; set; }
        public int TypeId { get; set; }
        public int ProductId { get; set; }
        public int IssueTypeId { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> OwnerId { get; set; }
        public string PackageId { get; set; }
        public int ProductCategoryId { get; set; }
        public string AssignedTo { get; set; }
        public string PrevAssignee { get; set; }
        public string NewAssignee { get; set; }
        public string PrevStatus { get; set; }
        public string NewStatus { get; set; }
    
        public virtual tic_Departments tic_Departments { get; set; }
        public virtual tic_IssueTypes tic_IssueTypes { get; set; }
        public virtual tic_TicketStatuses tic_TicketStatuses { get; set; }
        public virtual tic_TicketTypes tic_TicketTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TicketsLog> TicketsLogs { get; set; }
    }
}
