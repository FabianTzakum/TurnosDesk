using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using TurnosDesk.Operator.Models;

namespace TurnosDesk.Operator.Services;

public sealed class TurnosDeskRealtimeClient
{
    private readonly HubConnection _connection;

    public event Action<QueueTicketResponse>? TicketCalled;

    public event Action<QueueTicketResponse>? TicketStarted;

    public event Action<QueueTicketResponse>? TicketCompleted;

    public event Action<QueueTicketResponse>? TicketNoShow;

    public event Action<QueueTicketResponse>? TicketCancelled;

    public event Action? ConnectionClosed;

    public TurnosDeskRealtimeClient(string apiBaseUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{apiBaseUrl.TrimEnd('/')}/hubs/queue")
            .WithAutomaticReconnect()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .Build();

        _connection.On<QueueTicketResponse>("TicketCalled", ticket => TicketCalled?.Invoke(ticket));
        _connection.On<QueueTicketResponse>("TicketStarted", ticket => TicketStarted?.Invoke(ticket));
        _connection.On<QueueTicketResponse>("TicketCompleted", ticket => TicketCompleted?.Invoke(ticket));
        _connection.On<QueueTicketResponse>("TicketNoShow", ticket => TicketNoShow?.Invoke(ticket));
        _connection.On<QueueTicketResponse>("TicketCancelled", ticket => TicketCancelled?.Invoke(ticket));

        _connection.Closed += _ =>
        {
            ConnectionClosed?.Invoke();
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            return;
        }

        await _connection.StartAsync();
        await _connection.InvokeAsync("JoinPublicDisplay");
    }

    public async Task StopAsync()
    {
        await _connection.StopAsync();
    }
}
