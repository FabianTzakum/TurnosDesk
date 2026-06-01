using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/queue-tickets")]
public sealed class QueueTicketsController : ControllerBase
{
    private readonly IQueueTicketService _queueTicketService;

    public QueueTicketsController(IQueueTicketService queueTicketService)
    {
        _queueTicketService = queueTicketService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<QueueTicketResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<QueueTicketResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] int? branchId,
        [FromQuery] int? serviceTypeId,
        [FromQuery] QueueTicketStatus? status,
        [FromQuery] DateOnly? serviceDate
    )
    {
        var tickets = await _queueTicketService.GetPagedAsync(
            paginationParams,
            branchId,
            serviceTypeId,
            status,
            serviceDate
        );

        return Ok(ApiResponse<PagedResponse<QueueTicketResponse>>.Ok(
            tickets,
            "Turnos consultados correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> GetById(int id)
    {
        var ticket = await _queueTicketService.GetByIdAsync(id);

        if (ticket is null)
        {
            return NotFound(ApiResponse<QueueTicketResponse>.Fail(
                "Turno no encontrado.",
                new[] { "No existe un turno con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<QueueTicketResponse>.Ok(
            ticket,
            "Turno consultado correctamente."
        ));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> Create(
        [FromBody] CreateQueueTicketRequest request
    )
    {
        var result = await _queueTicketService.CreateAsync(request);

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
}
