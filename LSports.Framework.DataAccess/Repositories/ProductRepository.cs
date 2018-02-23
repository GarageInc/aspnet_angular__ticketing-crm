using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private const string Admin = "Admin";

        public IList<Product> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from product in model.tic_Products
                         join icon in model.tic_Icons on product.IconId equals icon.Id
                         where product.IsActive
                         orderby product.Name
                         select new Product
                         {
                             Id = product.Id,
                             Name = product.Name,
                             Icon = new _Icon
                             {
                                 Id = icon.Id,
                                 Icon = icon.Icon,
                                 Name = icon.Name
                             }
                         }).ToList();

                return q;
            }
        }


        public Product Insert(Product product)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_Products()
                {
                    Name = product.Name,
                    CreatedBy = Admin,
                    CreationDate = DateTime.Now,
                    IconId = product.Icon.Id,
                    IsActive = true
                };

                model.tic_Products.Add(newRecord);
                model.SaveChanges();
                product.Id = newRecord.Id;

                return product;
            }
        }


        public Product Update(Product product)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_Products.First(rec => rec.Id == product.Id);
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.IconId = product.Icon.Id;
                recordToEdit.Name = product.Name;

                model.SaveChanges();

                return product;

            }
        }


        public void Delete(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToDelete = model.tic_Products.First(rec => rec.Id == id);
                recordToDelete.UpdatedBy = Admin;
                recordToDelete.LastUpdate = DateTime.Now;
                recordToDelete.IsActive = false;

                model.SaveChanges();
            }
        }
    }
}
