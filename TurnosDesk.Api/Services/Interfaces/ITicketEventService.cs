using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.TicketEvents;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface ITicketEventService
{
    Task<PagedResponse<TicketEventResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? queueTicketId,
        int? serviceModuleId,
        TicketEventType? eventType,
        DateTimeOffset? from,
        DateTimeOffset? to
    );

    Task<TicketEventResponse?> GetByIdAsync(int id);

    Task<IReadOnlyCollection<TicketEventResponse>> GetByTicketIdAsync(int queueTicketId);
}
