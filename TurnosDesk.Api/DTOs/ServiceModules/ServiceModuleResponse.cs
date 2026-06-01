using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceModules;

public sealed record ServiceModuleResponse(
    int Id,
    int BranchId,
    string BranchName,
    int? ServiceAreaId,
    string? ServiceAreaName,
    string Code,
    string Name,
    ServiceModuleType Type,
    ServiceModuleStatus Status,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
