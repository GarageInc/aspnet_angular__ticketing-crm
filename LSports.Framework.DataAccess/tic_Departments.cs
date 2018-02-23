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
    
    public partial class tic_Departments
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tic_Departments()
        {
            this.Tickets = new HashSet<Ticket>();
            this.tic_UserDepartments = new HashSet<tic_UserDepartments>();
            this.tic_TicketWorkflow = new HashSet<tic_TicketWorkflow>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanSeeCustomerDetails { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public string UpdatedBy { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ticket> Tickets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tic_UserDepartments> tic_UserDepartments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tic_TicketWorkflow> tic_TicketWorkflow { get; set; }
    }
}