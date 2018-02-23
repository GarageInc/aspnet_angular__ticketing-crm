using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSports.Framework.Models.CustomClasses
{
    public class Staff
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdministrator { get; set; }
        public string Password { get; set; }
    }
}
