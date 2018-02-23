using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSports.Framework.Models;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface ITicketWorkflowRepository
    {
        IList<TicketWorkflow> GetList();


        TicketWorkflow Insert(TicketWorkflow workflow);


        TicketWorkflow Update(TicketWorkflow workflow);


        void Delete(int id);

        void UpdateSortOrder(IList<SortOrderItem> sortOrder);

        TicketWorkflow GetFirstMathingRule(TicketModel record);

        IList<Error> Validate(TicketWorkflow record);
    }
}
