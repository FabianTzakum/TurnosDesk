namespace TurnosDesk.Api.Domain.Enums;

public enum TicketEventType
{
    Created = 1,
    Called = 2,
    ServiceStarted = 3,
    ServiceCompleted = 4,
    Cancelled = 5,
    MarkedAsNoShow = 6,
    Recalled = 7
}
