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
    
    public partial class Customer
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public Nullable<int> CountryId { get; set; }
        public string City { get; set; }
        public string PostalIndex { get; set; }
        public string PostalAddress { get; set; }
        public string WebSite { get; set; }
        public string Phones { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public bool IsActive { get; set; }
        public int StatusId { get; set; }
        public string EMail { get; set; }
        public int ClassificationId { get; set; }
        public Nullable<System.DateTime> SubscriptionStartDate { get; set; }
        public Nullable<System.DateTime> SubscriptionEndDate { get; set; }
        public bool IsDeposit { get; set; }
        public string Notes { get; set; }
        public System.DateTime LastUpdate { get; set; }
        public int PaymentPeriod { get; set; }
        public Nullable<int> BookieId { get; set; }
        public Nullable<System.DateTime> PaidTillDate { get; set; }
    }
}