using System.Collections.Generic;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IFormTemplateRepository
    {
        IList<FormTemplate> GetList();


        FormTemplate Insert(FormTemplate template);


        FormTemplate Update(FormTemplate template);


        void Delete(int id);

        void UpdateSortOrder(IList<SortOrderItem> sortOrder);

        IList<FormTemplateCustomField> GetCustomFields(int formtemplateId);

        void UpdateCustomFields(int formTemplateId, IList<FormTemplateCustomField> fields);

        FormTemplate GetFirstMatchingTemplate(TicketModel record);

        IList<Error> Validate(FormTemplate record);
    }
}
