using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public HealthController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthStatusResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<HealthStatusResponse>> Get()
    {
        var applicationName = _configuration["Application:Name"] ?? "TurnosDesk API";
        var version = _configuration["Application:Version"] ?? "1.0.0";

        var response = new HealthStatusResponse(
            Application: applicationName,
            Version: version,
            Environment: _environment.EnvironmentName,
            Status: "Healthy",
            ServerTime: DateTimeOffset.UtcNow
        );

        return Ok(ApiResponse<HealthStatusResponse>.Ok(
            response,
            "TurnosDesk API está disponible."
        ));
    }
}

public sealed record HealthStatusResponse(
    string Application,
    string Version,
    string Environment,
    string Status,
    DateTimeOffset ServerTime
);
