using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSports.Framework.Models.CustomClasses
{
    public class CustomerModel
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public int? CountryId { get; set; }
        public string City { get; set; }
        public string PostalIndex { get; set; }
        public string PostalAddress { get; set; }
        public string WebSite { get; set; }
        public string Phones { get; set; }
        public string EMail { get; set; }
        public bool IsDeposit { get; set; }
        public string Notes { get; set; }
        public int PaymentPeriod { get; set; }
    }
}
