using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.DTOs.SystemCatalogs;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/system-catalogs")]
public sealed class SystemCatalogsController : ControllerBase
{
    private readonly ISystemCatalogService _systemCatalogService;

    public SystemCatalogsController(ISystemCatalogService systemCatalogService)
    {
        _systemCatalogService = systemCatalogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SystemCatalogsResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<SystemCatalogsResponse>> GetCatalogs()
    {
        var catalogs = _systemCatalogService.GetCatalogs();

        return Ok(ApiResponse<SystemCatalogsResponse>.Ok(
            catalogs,
            "Catálogos del sistema consultados correctamente."
        ));
    }
}
