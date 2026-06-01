using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.Branches;

public sealed record BranchResponse(
    int Id,
    string Code,
    string Name,
    string? Address,
    string? PhoneNumber,
    BranchStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
