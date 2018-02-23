using System;
using System.Collections.Generic;

namespace LSports.Framework.Models.CustomClasses
{
    public class TicketModel
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public int CustomerId { get; set; }
        public int CustomerUserId { get; set; }
        public int PriorityId { get; set; }
        public int TypeId { get; set; }
        public int ProductId { get; set; }
        public int ProductCategoryId { get; set; }
        public int IssueTypeId { get; set; }
        public int? DepartmentId { get; set; }
        public string AssignedTo { get; set; }
        public string PrevAssignee { get; set; }
        public string NewAssignee { get; set; }
        public string PrevStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime CreationDate { get; set; }

        public CustomUser AssignedToUser { get; set; }
        public Department Department { get; set; }
        public Product Product { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public IssueType IssueType { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public TicketType TicketType { get; set; }
        public CustomerModel Customer { get; set; }
        public CustomerContactModel CustomerContact { get; set; }
        //public Customer 
        public IList<TicketLog> TicketLogs { get; set; }
        public string PackageId { get; set; }
    }
}
