using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceModules;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/service-modules")]
public sealed class ServiceModulesController : ControllerBase
{
    private readonly IServiceModuleService _serviceModuleService;

    public ServiceModulesController(IServiceModuleService serviceModuleService)
    {
        _serviceModuleService = serviceModuleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ServiceModuleResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<ServiceModuleResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] int? branchId,
        [FromQuery] int? serviceAreaId,
        [FromQuery] ServiceModuleType? type,
        [FromQuery] ServiceModuleStatus? status
    )
    {
        var modules = await _serviceModuleService.GetPagedAsync(
            paginationParams,
            branchId,
            serviceAreaId,
            type,
            status
        );

        return Ok(ApiResponse<PagedResponse<ServiceModuleResponse>>.Ok(
            modules,
            "Módulos de atención consultados correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceModuleResponse>>> GetById(int id)
    {
        var module = await _serviceModuleService.GetByIdAsync(id);

        if (module is null)
        {
            return NotFound(ApiResponse<ServiceModuleResponse>.Fail(
                "Módulo de atención no encontrado.",
                new[] { "No existe un módulo de atención con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<ServiceModuleResponse>.Ok(
            module,
            "Módulo de atención consultado correctamente."
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceModuleResponse>>> Create(
        [FromBody] CreateServiceModuleRequest request
    )
    {
        var result = await _serviceModuleService.CreateAsync(request);

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
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceModuleResponse>>> Update(
        int id,
        [FromBody] UpdateServiceModuleRequest request
    )
    {
        var result = await _serviceModuleService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ServiceModuleResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceModuleResponse>>> ChangeStatus(
        int id,
        [FromBody] ChangeServiceModuleStatusRequest request
    )
    {
        var result = await _serviceModuleService.ChangeStatusAsync(id, request.Status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
