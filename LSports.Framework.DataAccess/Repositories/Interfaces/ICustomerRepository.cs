using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        CustomerContactModel GetCustomerContactByUserId(string userId);

        IList<CustomerContactModel> GetCustomerContactsByCustomerId(int customerId);

        CustomerContactModel AddCustomerContact(CustomerContactModel recordToAdd);

        CustomerContactModel GetCustomerContactByEmail(string email);
    }
}
