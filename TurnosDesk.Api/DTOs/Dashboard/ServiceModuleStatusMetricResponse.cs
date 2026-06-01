using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.Dashboard;

public sealed record ServiceModuleStatusMetricResponse(
    ServiceModuleStatus Status,
    int TotalModules
);
