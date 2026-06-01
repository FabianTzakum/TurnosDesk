using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.TicketEvents;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class TicketEventService : ITicketEventService
{
    private readonly TurnosDeskDbContext _dbContext;

    public TicketEventService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<TicketEventResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? queueTicketId,
        int? serviceModuleId,
        TicketEventType? eventType,
        DateTimeOffset? from,
        DateTimeOffset? to
    )
    {
        var query = _dbContext.TicketEvents
            .AsNoTracking()
            .Include(ticketEvent => ticketEvent.QueueTicket)
            .Include(ticketEvent => ticketEvent.ServiceModule)
            .AsQueryable();

        if (queueTicketId.HasValue)
        {
            query = query.Where(ticketEvent => ticketEvent.QueueTicketId == queueTicketId.Value);
        }

        if (serviceModuleId.HasValue)
        {
            query = query.Where(ticketEvent => ticketEvent.ServiceModuleId == serviceModuleId.Value);
        }

        if (eventType.HasValue)
        {
            query = query.Where(ticketEvent => ticketEvent.EventType == eventType.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(ticketEvent => ticketEvent.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(ticketEvent => ticketEvent.CreatedAt <= to.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(ticketEvent =>
                ticketEvent.Description.Contains(search) ||
                (ticketEvent.CreatedBy != null && ticketEvent.CreatedBy.Contains(search)) ||
                (ticketEvent.QueueTicket != null && ticketEvent.QueueTicket.Folio.Contains(search)) ||
                (ticketEvent.ServiceModule != null && ticketEvent.ServiceModule.Name.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(ticketEvent => ticketEvent.CreatedAt)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(ticketEvent => ToResponse(ticketEvent))
            .ToListAsync();

        return PagedResponse<TicketEventResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<TicketEventResponse?> GetByIdAsync(int id)
    {
        var ticketEvent = await _dbContext.TicketEvents
            .AsNoTracking()
            .Include(item => item.QueueTicket)
            .Include(item => item.ServiceModule)
            .FirstOrDefaultAsync(item => item.Id == id);

        return ticketEvent is null ? null : ToResponse(ticketEvent);
    }

    public async Task<IReadOnlyCollection<TicketEventResponse>> GetByTicketIdAsync(int queueTicketId)
    {
        var ticketExists = await _dbContext.QueueTickets
            .AnyAsync(ticket => ticket.Id == queueTicketId);

        if (!ticketExists)
        {
            return Array.Empty<TicketEventResponse>();
        }

        var events = await _dbContext.TicketEvents
            .AsNoTracking()
            .Include(ticketEvent => ticketEvent.QueueTicket)
            .Include(ticketEvent => ticketEvent.ServiceModule)
            .Where(ticketEvent => ticketEvent.QueueTicketId == queueTicketId)
            .OrderBy(ticketEvent => ticketEvent.CreatedAt)
            .Select(ticketEvent => ToResponse(ticketEvent))
            .ToListAsync();

        return events;
    }

    private static TicketEventResponse ToResponse(TicketEvent ticketEvent)
    {
        return new TicketEventResponse(
            ticketEvent.Id,
            ticketEvent.QueueTicketId,
            ticketEvent.QueueTicket?.Folio ?? "Turno no disponible",
            ticketEvent.ServiceModuleId,
            ticketEvent.ServiceModule?.Name,
            ticketEvent.EventType,
            ticketEvent.Description,
            ticketEvent.CreatedAt,
            ticketEvent.CreatedBy
        );
    }
}
