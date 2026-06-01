using TurnosDesk.Api.DTOs.QueueTickets;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IQueueRealtimeNotifier
{
    Task NotifyTicketCalledAsync(QueueTicketResponse ticket);

    Task NotifyTicketStartedAsync(QueueTicketResponse ticket);

    Task NotifyTicketCompletedAsync(QueueTicketResponse ticket);

    Task NotifyTicketNoShowAsync(QueueTicketResponse ticket);

    Task NotifyTicketCancelledAsync(QueueTicketResponse ticket);
}
