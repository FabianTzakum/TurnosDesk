using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.Branches;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/branches")]
public sealed class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<BranchResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<BranchResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] BranchStatus? status
    )
    {
        var branches = await _branchService.GetPagedAsync(paginationParams, status);

        return Ok(ApiResponse<PagedResponse<BranchResponse>>.Ok(
            branches,
            "Sucursales consultadas correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);

        if (branch is null)
        {
            return NotFound(ApiResponse<BranchResponse>.Fail(
                "Sucursal no encontrada.",
                new[] { "No existe una sucursal con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<BranchResponse>.Ok(
            branch,
            "Sucursal consultada correctamente."
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> Create([FromBody] CreateBranchRequest request)
    {
        var result = await _branchService.CreateAsync(request);

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
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> Update(
        int id,
        [FromBody] UpdateBranchRequest request
    )
    {
        var result = await _branchService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BranchResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> ChangeStatus(
        int id,
        [FromBody] ChangeBranchStatusRequest request
    )
    {
        var result = await _branchService.ChangeStatusAsync(id, request.Status);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
