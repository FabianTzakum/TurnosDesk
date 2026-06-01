using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IQueueTicketService
{
    Task<PagedResponse<QueueTicketResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        int? serviceTypeId,
        QueueTicketStatus? status,
        DateOnly? serviceDate
    );

    Task<QueueTicketResponse?> GetByIdAsync(int id);

    Task<ApiResponse<QueueTicketResponse>> CreateAsync(CreateQueueTicketRequest request);
}
