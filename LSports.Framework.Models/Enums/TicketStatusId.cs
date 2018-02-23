
namespace LSports.Framework.Models.Enums
{
    public enum TicketStatusId
    {
        Open = 1,
        InProgress = 2,
        WaitingForCustomerReply = 3,
        WaitingForSupportReply = 4,
        Fixed = 5,
        ClosedDidNotHaveAnyIssues = 6,
        ClosedIssueWasResolved = 7,
        ClosedIrrelevantAnymore = 8,
        ClosedByUser = 9,
        WaitingForStaffReply = 10
    }
}
