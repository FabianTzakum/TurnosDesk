using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceAreas;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/service-areas")]
public sealed class ServiceAreasController : ControllerBase
{
    private readonly IServiceAreaService _serviceAreaService;

    public ServiceAreasController(IServiceAreaService serviceAreaService)
    {
        _serviceAreaService = serviceAreaService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ServiceAreaResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ServiceAreaResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] int? branchId,
        [FromQuery] ServiceAreaStatus? status
    )
    {
        var areas = await _serviceAreaService.GetPagedAsync(
            paginationParams,
            branchId,
            status
        );

        return Ok(ApiResponse<PagedResponse<ServiceAreaResponse>>.Ok(
            areas,
            "Áreas de atención consultadas correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceAreaResponse>>> GetById(int id)
    {
        var area = await _serviceAreaService.GetByIdAsync(id);

        if (area is null)
        {
            return NotFound(ApiResponse<ServiceAreaResponse>.Fail(
                "Área de atención no encontrada.",
                new[] { "No existe un área de atención con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<ServiceAreaResponse>.Ok(
            area,
            "Área de atención consultada correctamente."
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceAreaResponse>>> Create(
        [FromBody] CreateServiceAreaRequest request
    )
    {
        var result = await _serviceAreaService.CreateAsync(request);

        if (!result.Success || result.Data is null)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data.Id },
            result
        );
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceAreaResponse>>> Update(
        int id,
        [FromBody] UpdateServiceAreaRequest request
    )
    {
        var result = await _serviceAreaService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceAreaResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceAreaResponse>>> ChangeStatus(
        int id,
        [FromBody] ChangeServiceAreaStatusRequest request
    )
    {
        var result = await _serviceAreaService.ChangeStatusAsync(id, request.Status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
