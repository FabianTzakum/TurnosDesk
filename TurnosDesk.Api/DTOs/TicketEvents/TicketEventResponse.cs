using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.TicketEvents;

public sealed record TicketEventResponse(
    int Id,
    int QueueTicketId,
    string TicketFolio,
    int? ServiceModuleId,
    string? ServiceModuleName,
    TicketEventType EventType,
    string Description,
    DateTimeOffset CreatedAt,
    string? CreatedBy
);
