using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceAreas;

public sealed record ServiceAreaResponse(
    int Id,
    int BranchId,
    string BranchName,
    string Code,
    string Name,
    string? Description,
    ServiceAreaStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
