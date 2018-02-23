using LSports.Framework.Models;

namespace LSports.Services.Interfaces
{
    public interface ITicketWorkflowService
    {
        TicketWorkflow Insert(TicketWorkflow workflow);
        TicketWorkflow Update(TicketWorkflow workflow);

    }
}
