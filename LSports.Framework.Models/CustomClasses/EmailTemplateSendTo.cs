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
    
    public class EmailTemplateSendTo
    {
        public int Id { get; set; }
        public int EmailSendToId { get; set; }
        public int EmailTemplateId { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public string UpdatedBy { get; set; }
    
        public EmailSendTo EmailSendTo { get; set; }
        public MailTemplate EmailTemplate { get; set; }
    }
}