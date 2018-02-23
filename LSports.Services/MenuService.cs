using System.Collections.Generic;
using System.Linq;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.Models.Enums;
using LSports.Services.Interfaces;

namespace LSports.Services
{
    public class MenuService : IMenuService
    {
        private readonly IUserDepartmentRepository _userDepartmentRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;


        public MenuService() : this(new UserDepartmentRepository(), new TicketRepository(), new UserRepository(), new DepartmentRepository())
        {
            
        }

        public MenuService(IUserDepartmentRepository userDepartmentRepository, ITicketRepository ticketRepository,
            IUserRepository userRepository, IDepartmentRepository departmentRepository)
        {
            _userDepartmentRepository = userDepartmentRepository;
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _departmentRepository = departmentRepository;
        }



        public IList<RootMenuItem> GetMenuItems(string username, bool isAdmin)
        {

            var user = _userRepository.GetUserByName(username);


            List<UserDepartment> userDepartments;

            if (!isAdmin)
                userDepartments = _userDepartmentRepository.GetListByUserId(user.Id).ToList();
            else
                userDepartments = _departmentRepository.GetList()
                        .Select(
                            rec =>
                                new UserDepartment
                                {
                                    Department = new Department {Id = rec.Id, Name = rec.Name},
                                    DepartmentRole = new DepartmentRole {Id = DepartmentRoleId.DepartmentManager}
                                }).ToList();


            var model = new List<RootMenuItem>();

            int opened = 0, closed = 0, assignedToMe = 0, unassigned = 0, all = 0;

            foreach (var userDepartment in userDepartments)
            {
                var departmentIds = new List<int>
                {
                    userDepartment.Department.Id
                };
                var openedTickets = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds, new List<TicketStatusId> {TicketStatusId.Open}).Count;

                var ticketsAssignedToMe = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds, user.Id).Count;

                var ticketsUnassigned = _ticketRepository.GetTicketsByDepartmentIdAndAssignee(
                    departmentIds, null).Count;

                var closedTickets = _ticketRepository.GetTicketsByDepartmentIdAndStatuses(
                    departmentIds,
                    new List<TicketStatusId>
                    {
                        TicketStatusId.ClosedByUser,
                        TicketStatusId.ClosedDidNotHaveAnyIssues,
                        TicketStatusId.ClosedIrrelevantAnymore,
                        TicketStatusId.ClosedIssueWasResolved
                    }).Count;

                var allTickets = _ticketRepository.GetTicketsByDepartmentId(departmentIds).Count();

                if (userDepartment.DepartmentRole.Id == DepartmentRoleId.Staff)
                {
                    model.Add(new RootMenuItem
                    {
                        Url = null,
                        Title = userDepartment.Department.Name,
                        Count = openedTickets + ticketsAssignedToMe + ticketsUnassigned,
                        Subitems = new List<MenuItem>
                        {
                            new MenuItem
                            {
                                Title = "Opened Tickets",
                                Count = openedTickets,
                                Parameters  = new
                                {
                                    Status = "open",
                                    DepartmentId = userDepartment.Department.Id
                                }
                            },
                            new MenuItem
                            {
                                Title = "Assigned to me",
                                Count = ticketsAssignedToMe,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id,
                                    Assignee = "assignedtome"
                                }
                            },
                            new MenuItem
                            {
                                Title = "Unassigned",
                                Count = ticketsUnassigned,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id,
                                    Assignee = "unassigned"
                                }
                            }
                        }
                    });

                    opened += openedTickets;
                    assignedToMe += ticketsAssignedToMe;
                    unassigned += ticketsUnassigned;
                    closed += closedTickets;
                    all += allTickets;
                }
                else
                //if Department Mananeger
                {

                    model.Add(new RootMenuItem
                    {
                        Url = null,
                        Title = userDepartment.Department.Name,
                        Count = allTickets,//openedTickets + ticketsAssignedToMe + ticketsUnassigned ,
                        Subitems = new List<MenuItem>
                        {
                            new MenuItem
                            {
                                Title = "All Tickets",
                                Count = allTickets,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id
                                }
                            },

                            new MenuItem
                            {
                                Title = "Opened Tickets",
                                Count = openedTickets,
                                Parameters  = new
                                {
                                    Status = "open",
                                    DepartmentId = userDepartment.Department.Id
                                }
                            },
                             new MenuItem
                            {
                                Title = "Finished Tickets",
                                Count = closedTickets,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id,
                                    Status = "closed"
                                }
                            },
                            new MenuItem
                            {
                                Title = "Assigned to me",
                                Count = ticketsAssignedToMe,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id,
                                    Assignee = "assignedtome"
                                }
                            },
                            new MenuItem
                            {
                                Title = "Unassigned",
                                Count = ticketsUnassigned,
                                Parameters  = new
                                {
                                    DepartmentId = userDepartment.Department.Id,
                                    Assignee = "unassigned"
                                }
                            }
                        }
                    });


                    opened += openedTickets;
                    assignedToMe += ticketsAssignedToMe;
                    unassigned += ticketsUnassigned;
                    closed += closedTickets;
                    all += allTickets;
                }

            }

            model.Insert(0, new RootMenuItem
            {
                Url = null,
                Title = "All departments",
                Count = all,//openedTickets + ticketsAssignedToMe + ticketsUnassigned ,
                Subitems = new List<MenuItem>
                        {
                            new MenuItem
                            {
                                Title = "All Tickets",
                                Count = all,
                                Parameters  = new
                                {
                                }
                            },

                            new MenuItem
                            {
                                Title = "Opened Tickets",
                                Count = opened,
                                Parameters  = new
                                {
                                    Status = "open"
                                }
                            },
                             new MenuItem
                            {
                                Title = "Finished Tickets",
                                Count = closed,
                                Parameters  = new
                                {
                                    Status = "closed"
                                }
                            },
                            new MenuItem
                            {
                                Title = "Assigned to me",
                                Count = assignedToMe,
                                Parameters  = new
                                {
                                    Assignee = "assignedtome"
                                }
                            },
                            new MenuItem
                            {
                                Title = "Unassigned",
                                Count = unassigned,
                                Parameters  = new
                                {
                                    Assignee = "unassigned"
                                }
                            }
                        }
            });

            return model;

        }
    }
}
