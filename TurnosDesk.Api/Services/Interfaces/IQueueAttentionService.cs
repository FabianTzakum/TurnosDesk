using TurnosDesk.Api.DTOs.QueueAttention;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IQueueAttentionService
{
    Task<ApiResponse<QueueTicketResponse>> CallNextAsync(CallNextQueueTicketRequest request);

    Task<ApiResponse<QueueTicketResponse>> RecallAsync(int ticketId, RecallQueueTicketRequest request);

    Task<ApiResponse<QueueTicketResponse>> StartServiceAsync(int ticketId, StartQueueTicketRequest request);

    Task<ApiResponse<QueueTicketResponse>> CompleteServiceAsync(int ticketId, CompleteQueueTicketRequest request);

    Task<ApiResponse<QueueTicketResponse>> MarkAsNoShowAsync(int ticketId, MarkQueueTicketNoShowRequest request);

    Task<ApiResponse<QueueTicketResponse>> CancelAsync(int ticketId, CancelQueueTicketRequest request);
}
