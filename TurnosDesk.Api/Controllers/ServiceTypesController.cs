using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceTypes;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/service-types")]
public sealed class ServiceTypesController : ControllerBase
{
    private readonly IServiceTypeService _serviceTypeService;

    public ServiceTypesController(IServiceTypeService serviceTypeService)
    {
        _serviceTypeService = serviceTypeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ServiceTypeResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ServiceTypeResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] int? serviceAreaId,
        [FromQuery] ServicePriority? priority,
        [FromQuery] ServiceTypeStatus? status
    )
    {
        var serviceTypes = await _serviceTypeService.GetPagedAsync(
            paginationParams,
            serviceAreaId,
            priority,
            status
        );

        return Ok(ApiResponse<PagedResponse<ServiceTypeResponse>>.Ok(
            serviceTypes,
            "Servicios consultados correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceTypeResponse>>> GetById(int id)
    {
        var serviceType = await _serviceTypeService.GetByIdAsync(id);

        if (serviceType is null)
        {
            return NotFound(ApiResponse<ServiceTypeResponse>.Fail(
                "Servicio no encontrado.",
                new[] { "No existe un servicio con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<ServiceTypeResponse>.Ok(
            serviceType,
            "Servicio consultado correctamente."
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceTypeResponse>>> Create(
        [FromBody] CreateServiceTypeRequest request
    )
    {
        var result = await _serviceTypeService.CreateAsync(request);

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
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceTypeResponse>>> Update(
        int id,
        [FromBody] UpdateServiceTypeRequest request
    )
    {
        var result = await _serviceTypeService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceTypeResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceTypeResponse>>> ChangeStatus(
        int id,
        [FromBody] ChangeServiceTypeStatusRequest request
    )
    {
        var result = await _serviceTypeService.ChangeStatusAsync(id, request.Status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
