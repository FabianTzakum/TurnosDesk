using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceTypes;

public sealed record ServiceTypeResponse(
    int Id,
    int? ServiceAreaId,
    string? ServiceAreaName,
    string Code,
    string Prefix,
    string Name,
    string? Description,
    int EstimatedMinutes,
    ServicePriority Priority,
    ServiceTypeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
