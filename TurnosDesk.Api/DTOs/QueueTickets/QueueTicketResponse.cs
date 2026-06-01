using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.QueueTickets;

public sealed record QueueTicketResponse(
    int Id,
    int BranchId,
    string BranchName,
    int ServiceTypeId,
    string ServiceTypeName,
    string ServiceTypePrefix,
    int? ServiceModuleId,
    string? ServiceModuleName,
    string Folio,
    int DailySequence,
    QueueTicketStatus Status,
    string? CustomerName,
    string? CustomerReference,
    string? Notes,
    DateOnly ServiceDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CalledAt,
    DateTimeOffset? ServiceStartedAt,
    DateTimeOffset? ServiceCompletedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? NoShowAt
);
