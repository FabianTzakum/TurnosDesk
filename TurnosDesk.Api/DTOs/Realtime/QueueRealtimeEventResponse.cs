using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.Realtime;

public sealed record QueueRealtimeEventResponse(
    string EventName,
    int TicketId,
    string Folio,
    QueueTicketStatus Status,
    int BranchId,
    string BranchName,
    int ServiceTypeId,
    string ServiceTypeName,
    int? ServiceModuleId,
    string? ServiceModuleName,
    DateTimeOffset OccurredAt
);
