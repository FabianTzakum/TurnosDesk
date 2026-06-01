using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.Dashboard;

public sealed record LatestCalledTicketResponse(
    int TicketId,
    string Folio,
    QueueTicketStatus Status,
    string ServiceTypeName,
    string? ServiceModuleName,
    DateTimeOffset? CalledAt
);
