//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.Models
{
    using System;
    using System.Collections.Generic;
    
    public class FormTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? TicketTypeId { get; set; }
        public int? ProductId { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? IssueTypeId { get; set; }
        public int SortOrder { get; set; }
    
        public IssueType IssueType { get; set; }
        public ProductCategory ProductCategory { get; set; }
        public Product Product { get; set; }
        public TicketType TicketType { get; set; }

        public IList<FormTemplateCustomField> CustomFields { get; set; }
    }
}
