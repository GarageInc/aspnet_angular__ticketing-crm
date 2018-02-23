using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class DepartmentRoleRepository : IDepartmentRoleRepository
    {
            public IList<DepartmentRole> GetList()
            {

                using (var model = new gb_ts_stagingEntities())
                {
                    var q = model.tic_DepartmentRoles.Select(rec => new DepartmentRole
                    {
                        Id = rec.Id,
                        Name = rec.RoleName,
                    }).OrderBy(rec=>rec.Name);

                    return q.ToList();
                }
            }
        }
}
