using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IProductCategoryRepository
    {
        IList<ProductCategory> GetList();


        Product Insert(Product productCategory);


        Product Update(Product productCategory);


        void Delete(int id);
    }
}
