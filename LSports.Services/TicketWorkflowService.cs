using System;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class TicketWorkflowService : ITicketWorkflowService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketWorkflowService() : this(new TicketRepository())
        {
            
        }

        public TicketWorkflowService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public TicketWorkflow Insert(TicketWorkflow workflow)
        {
            throw new NotImplementedException();
        }

        public TicketWorkflow Update(TicketWorkflow workflow)
        {
            throw new NotImplementedException();
        }
    }
}
