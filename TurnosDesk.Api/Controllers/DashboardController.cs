using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.DTOs.Dashboard;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummary(
        [FromQuery] int? branchId,
        [FromQuery] DateOnly? serviceDate
    )
    {
        var summary = await _dashboardService.GetSummaryAsync(branchId, serviceDate);

        return Ok(ApiResponse<DashboardSummaryResponse>.Ok(
            summary,
            "Resumen operativo consultado correctamente."
        ));
    }
}
