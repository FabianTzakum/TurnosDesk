using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.TicketEvents;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/ticket-events")]
public sealed class TicketEventsController : ControllerBase
{
    private readonly ITicketEventService _ticketEventService;

    public TicketEventsController(ITicketEventService ticketEventService)
    {
        _ticketEventService = ticketEventService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TicketEventResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TicketEventResponse>>>> GetPaged(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] int? queueTicketId,
        [FromQuery] int? serviceModuleId,
        [FromQuery] TicketEventType? eventType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to
    )
    {
        var events = await _ticketEventService.GetPagedAsync(
            paginationParams,
            queueTicketId,
            serviceModuleId,
            eventType,
            from,
            to
        );

        return Ok(ApiResponse<PagedResponse<TicketEventResponse>>.Ok(
            events,
            "Eventos consultados correctamente."
        ));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TicketEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketEventResponse>>> GetById(int id)
    {
        var ticketEvent = await _ticketEventService.GetByIdAsync(id);

        if (ticketEvent is null)
        {
            return NotFound(ApiResponse<TicketEventResponse>.Fail(
                "Evento no encontrado.",
                new[] { "No existe un evento con el identificador solicitado." }
            ));
        }

        return Ok(ApiResponse<TicketEventResponse>.Ok(
            ticketEvent,
            "Evento consultado correctamente."
        ));
    }

    [HttpGet("by-ticket/{queueTicketId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TicketEventResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<TicketEventResponse>>>> GetByTicketId(
        int queueTicketId
    )
    {
        var events = await _ticketEventService.GetByTicketIdAsync(queueTicketId);

        if (events.Count == 0)
        {
            return Ok(ApiResponse<IReadOnlyCollection<TicketEventResponse>>.Ok(
                events,
                "No se encontraron eventos para el turno solicitado."
            ));
        }

        return Ok(ApiResponse<IReadOnlyCollection<TicketEventResponse>>.Ok(
            events,
            "Historial del turno consultado correctamente."
        ));
    }
}
