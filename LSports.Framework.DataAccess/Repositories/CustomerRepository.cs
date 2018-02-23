using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        public CustomerContactModel AddCustomerContact(CustomerContactModel recordToAdd)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new CustomerContact
                {
                    EMail = recordToAdd.EMail,
                    CustomerId = recordToAdd.CustomerId,
                    Priority = recordToAdd.Priority,
                    FirstName = recordToAdd.FirstName??"",
                    LastName = recordToAdd.LastName??"",
                    Phones = recordToAdd.Phones??"",
                    Skype = recordToAdd.Skype??"",
                    UserId = recordToAdd.UserId??""
                };

                model.CustomerContacts.Add(newRecord);
                model.SaveChanges();
                recordToAdd.Id = newRecord.Id;

                return recordToAdd;
            }
        }

        public CustomerContactModel GetCustomerContactByEmail(string email)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                return model.CustomerContacts.Select(
                    rec=>new CustomerContactModel
                    {
                        Id = rec.Id,
                        LastName = rec.LastName,
                        FirstName = rec.FirstName,
                        UserId = rec.UserId,
                        CustomerId = rec.CustomerId,
                        EMail = rec.EMail,
                        Priority = rec.Priority,
                        Skype = rec.Skype,
                        Phones = rec.Phones
                    }).FirstOrDefault(rec => rec.EMail == email);
            }
        }

        public CustomerContactModel GetCustomerContactByUserId(string userId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customerContact in model.CustomerContacts
                         where customerContact.UserId == userId
                         select new CustomerContactModel
                         {
                             Id = customerContact.Id,
                             CustomerId = customerContact.CustomerId,
                             Priority = customerContact.Priority,
                         }).FirstOrDefault();

                return q;
            }
        }

        public IList<CustomerContactModel> GetCustomerContactsByCustomerId(int customerId)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from customerContact in model.CustomerContacts
                         where customerContact.CustomerId == customerId
                         select new CustomerContactModel
                         {
                             Id = customerContact.Id,
                             CustomerId = customerContact.CustomerId,
                             EMail = customerContact.EMail,
                             Priority = customerContact.Priority,
                         }).ToList();

                return q;
            }
        }
    }
}