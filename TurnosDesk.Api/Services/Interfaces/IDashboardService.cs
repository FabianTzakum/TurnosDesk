using TurnosDesk.Api.DTOs.Dashboard;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(
        int? branchId,
        DateOnly? serviceDate
    );
}
