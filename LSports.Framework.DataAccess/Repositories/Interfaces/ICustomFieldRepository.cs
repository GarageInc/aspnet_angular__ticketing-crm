using System.Collections.Generic;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ICustomFieldRepository
    {
        IList<CustomField> GetList(int? rootId);

        IList<CustomFieldType> GetCustomFieldTypes();

        CustomField Insert(CustomField customField);


        CustomField Update(CustomField customField);


        void Delete(int id);

        bool IsCustomFieldNameUnique(string name, int id);

        bool IsCustomFieldIdentifierUnique(string identifier, int id);

        void Update(CustomFieldCollection data);

        IList<CustomFieldDropdown> GetCustomFieldDropdowns();

        IList<CustomFieldIdentifier> GetCustomFieldIdentifiers();

        string IsCustomFieldValuesUnique(string name, string identifier, int id);
    }
}
