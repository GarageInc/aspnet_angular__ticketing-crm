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
    
    public partial class tic_DepartmentRoles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tic_DepartmentRoles()
        {
            this.tic_UserDepartments = new HashSet<tic_UserDepartments>();
        }
    
        public string Id { get; set; }
        public string RoleName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tic_UserDepartments> tic_UserDepartments { get; set; }
    }
}