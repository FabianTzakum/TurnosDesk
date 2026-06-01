using Microsoft.AspNetCore.SignalR;

namespace TurnosDesk.Api.Hubs;

public sealed class QueueHub : Hub
{
    public const string PublicDisplayGroup = "public-display";

    public async Task JoinPublicDisplay()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, PublicDisplayGroup);
    }

    public async Task LeavePublicDisplay()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, PublicDisplayGroup);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
