using Microsoft.AspNetCore.SignalR;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Hubs;
using TurnosDesk.Api.Services.Interfaces;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class QueueRealtimeNotifier : IQueueRealtimeNotifier
{
    private readonly IHubContext<QueueHub> _hubContext;

    public QueueRealtimeNotifier(IHubContext<QueueHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyTicketCalledAsync(QueueTicketResponse ticket)
    {
        await _hubContext.Clients
            .Group(QueueHub.PublicDisplayGroup)
            .SendAsync("TicketCalled", ticket);
    }

    public async Task NotifyTicketStartedAsync(QueueTicketResponse ticket)
    {
        await _hubContext.Clients
            .Group(QueueHub.PublicDisplayGroup)
            .SendAsync("TicketStarted", ticket);
    }

    public async Task NotifyTicketCompletedAsync(QueueTicketResponse ticket)
    {
        await _hubContext.Clients
            .Group(QueueHub.PublicDisplayGroup)
            .SendAsync("TicketCompleted", ticket);
    }

    public async Task NotifyTicketNoShowAsync(QueueTicketResponse ticket)
    {
        await _hubContext.Clients
            .Group(QueueHub.PublicDisplayGroup)
            .SendAsync("TicketNoShow", ticket);
    }

    public async Task NotifyTicketCancelledAsync(QueueTicketResponse ticket)
    {
        await _hubContext.Clients
            .Group(QueueHub.PublicDisplayGroup)
            .SendAsync("TicketCancelled", ticket);
    }
}
