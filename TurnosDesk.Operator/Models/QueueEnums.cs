namespace TurnosDesk.Operator.Models;

public enum QueueTicketStatus
{
    Pending = 1,
    Called = 2,
    InService = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}

public enum ServiceModuleStatus
{
    Active = 1,
    Inactive = 2,
    Busy = 3,
    Paused = 4,
    Maintenance = 5
}
