using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IProductRepository
    {
        IList<Product> GetList();


        Product Insert(Product product);


        Product Update(Product product);


        void Delete(int id);
    }
}
