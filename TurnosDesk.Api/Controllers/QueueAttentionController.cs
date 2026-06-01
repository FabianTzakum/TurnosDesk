using Microsoft.AspNetCore.Mvc;
using TurnosDesk.Api.DTOs.QueueAttention;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Controllers;

[ApiController]
[Route("api/queue-attention")]
public sealed class QueueAttentionController : ControllerBase
{
    private readonly IQueueAttentionService _queueAttentionService;

    public QueueAttentionController(IQueueAttentionService queueAttentionService)
    {
        _queueAttentionService = queueAttentionService;
    }

    [HttpPost("call-next")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> CallNext(
        [FromBody] CallNextQueueTicketRequest request
    )
    {
        var result = await _queueAttentionService.CallNextAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{ticketId:int}/recall")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> Recall(
        int ticketId,
        [FromBody] RecallQueueTicketRequest request
    )
    {
        var result = await _queueAttentionService.RecallAsync(ticketId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{ticketId:int}/start")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> StartService(
        int ticketId,
        [FromBody] StartQueueTicketRequest request
    )
    {
        var result = await _queueAttentionService.StartServiceAsync(ticketId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{ticketId:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> CompleteService(
        int ticketId,
        [FromBody] CompleteQueueTicketRequest request
    )
    {
        var result = await _queueAttentionService.CompleteServiceAsync(ticketId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{ticketId:int}/no-show")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> MarkAsNoShow(
        int ticketId,
        [FromBody] MarkQueueTicketNoShowRequest request
    )
    {
        var result = await _queueAttentionService.MarkAsNoShowAsync(ticketId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{ticketId:int}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<QueueTicketResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<QueueTicketResponse>>> Cancel(
        int ticketId,
        [FromBody] CancelQueueTicketRequest request
    )
    {
        var result = await _queueAttentionService.CancelAsync(ticketId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
