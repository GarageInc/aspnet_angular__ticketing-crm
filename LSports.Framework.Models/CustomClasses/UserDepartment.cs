using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSports.Framework.Models.CustomClasses
{
    public class UserDepartment
    {
        public int Id { get; set; }
        public int? DepartmentId { get; set; }
        public string UserId { get; set; }
        public string DepartmentRoleId { get; set; }
        public CustomUser User { get; set; }
        public Department Department { get; set; }
        public DepartmentRole DepartmentRole { get; set; }
    }
}
