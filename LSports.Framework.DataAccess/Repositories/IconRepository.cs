using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class IconRepository : IIconRepository
    {
        public IList<_Icon> GetList()
        {

                using (var model = new gb_ts_stagingEntities())
                {
                    var q = model.tic_Icons.Select(rec => new _Icon()
                    {
                        Id = rec.Id,
                        Name = rec.Name,
                        Icon = rec.Icon
                    }).OrderBy(rec=>rec.Name);

                    return q.ToList();
                }
        }
    }
}
