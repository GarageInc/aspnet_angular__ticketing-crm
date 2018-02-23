using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private const string Admin = "Admin";

        public IList<ProductCategory> GetList()
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from productCategory in model.tic_ProductCategories
                         join icon in model.tic_Icons on productCategory.IconId equals icon.Id
                         where productCategory.IsActive
                         orderby productCategory.Name
                         select new ProductCategory
                         {
                             Id = productCategory.Id,
                             Name = productCategory.Name,
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


        public Product Insert(Product productCategory)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_Products()
                {
                    Name = productCategory.Name,
                    CreatedBy = Admin,
                    CreationDate = DateTime.Now,
                    IconId = productCategory.Icon.Id,
                    IsActive = true
                };

                model.tic_Products.Add(newRecord);
                model.SaveChanges();
                productCategory.Id = newRecord.Id;

                return productCategory;
            }
        }


        public Product Update(Product productCategory)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var recordToEdit = model.tic_Products.First(rec => rec.Id == productCategory.Id);
                recordToEdit.UpdatedBy = "Admin";
                recordToEdit.LastUpdate = DateTime.Now;
                recordToEdit.IconId = productCategory.Icon.Id;
                recordToEdit.Name = productCategory.Name;

                model.SaveChanges();

                return productCategory;

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
