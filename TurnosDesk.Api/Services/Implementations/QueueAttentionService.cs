using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.QueueAttention;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class QueueAttentionService : IQueueAttentionService
{
    private readonly TurnosDeskDbContext _dbContext;

    public QueueAttentionService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<QueueTicketResponse>> CallNextAsync(CallNextQueueTicketRequest request)
    {
        var module = await _dbContext.ServiceModules
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == request.ServiceModuleId);

        if (module is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo llamar el siguiente turno.",
                new[] { "El módulo de atención indicado no existe." }
            );
        }

        if (module.BranchId != request.BranchId)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo llamar el siguiente turno.",
                new[] { "El módulo de atención no pertenece a la sucursal indicada." }
            );
        }

        if (module.Status != ServiceModuleStatus.Active)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo llamar el siguiente turno.",
                new[] { "El módulo de atención debe estar activo y disponible." }
            );
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _dbContext.QueueTickets
            .Include(ticket => ticket.Branch)
            .Include(ticket => ticket.ServiceType)
            .Include(ticket => ticket.ServiceModule)
            .Where(ticket =>
                ticket.BranchId == request.BranchId &&
                ticket.ServiceDate == today &&
                ticket.Status == QueueTicketStatus.Pending
            );

        if (request.ServiceTypeId.HasValue)
        {
            query = query.Where(ticket => ticket.ServiceTypeId == request.ServiceTypeId.Value);
        }

        var ticket = await query
            .OrderBy(ticket => ticket.DailySequence)
            .FirstOrDefaultAsync();

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No hay turnos pendientes para llamar.",
                new[] { "La cola no tiene turnos pendientes con los filtros seleccionados." }
            );
        }

        ticket.ServiceModuleId = module.Id;
        ticket.ServiceModule = module;
        ticket.Status = QueueTicketStatus.Called;
        ticket.CalledAt = DateTimeOffset.UtcNow;

        module.Status = ServiceModuleStatus.Busy;
        module.UpdatedAt = DateTimeOffset.UtcNow;

        AddEvent(
            ticket.Id,
            module.Id,
            TicketEventType.Called,
            $"Turno {ticket.Folio} llamado en {module.Name}.",
            request.OperatorName
        );

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Turno llamado correctamente."
        );
    }

    public async Task<ApiResponse<QueueTicketResponse>> RecallAsync(int ticketId, RecallQueueTicketRequest request)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId);

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo volver a llamar el turno.",
                new[] { "El turno solicitado no existe." }
            );
        }

        if (ticket.Status != QueueTicketStatus.Called)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo volver a llamar el turno.",
                new[] { "Solo se puede volver a llamar un turno que ya está en estado llamado." }
            );
        }

        var module = await _dbContext.ServiceModules
            .FirstOrDefaultAsync(item => item.Id == request.ServiceModuleId);

        if (module is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo volver a llamar el turno.",
                new[] { "El módulo de atención indicado no existe." }
            );
        }

        if (module.BranchId != ticket.BranchId)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo volver a llamar el turno.",
                new[] { "El módulo de atención no pertenece a la sucursal del turno." }
            );
        }

        ticket.ServiceModuleId = module.Id;
        ticket.CalledAt = DateTimeOffset.UtcNow;

        module.Status = ServiceModuleStatus.Busy;
        module.UpdatedAt = DateTimeOffset.UtcNow;

        AddEvent(
            ticket.Id,
            module.Id,
            TicketEventType.Recalled,
            $"Turno {ticket.Folio} llamado nuevamente en {module.Name}.",
            request.OperatorName
        );

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Turno llamado nuevamente correctamente."
        );
    }

    public async Task<ApiResponse<QueueTicketResponse>> StartServiceAsync(int ticketId, StartQueueTicketRequest request)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId);

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo iniciar la atención.",
                new[] { "El turno solicitado no existe." }
            );
        }

        if (ticket.Status != QueueTicketStatus.Called)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo iniciar la atención.",
                new[] { "Solo se puede iniciar la atención de un turno llamado." }
            );
        }

        ticket.Status = QueueTicketStatus.InService;
        ticket.ServiceStartedAt = DateTimeOffset.UtcNow;

        AddEvent(
            ticket.Id,
            ticket.ServiceModuleId,
            TicketEventType.ServiceStarted,
            $"Atención iniciada para el turno {ticket.Folio}.",
            request.OperatorName
        );

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Atención iniciada correctamente."
        );
    }

    public async Task<ApiResponse<QueueTicketResponse>> CompleteServiceAsync(
        int ticketId,
        CompleteQueueTicketRequest request
    )
    {
        var ticket = await GetTicketForUpdateAsync(ticketId);

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo finalizar la atención.",
                new[] { "El turno solicitado no existe." }
            );
        }

        if (ticket.Status != QueueTicketStatus.InService)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo finalizar la atención.",
                new[] { "Solo se puede finalizar un turno que está en atención." }
            );
        }

        ticket.Status = QueueTicketStatus.Completed;
        ticket.ServiceCompletedAt = DateTimeOffset.UtcNow;

        var description = string.IsNullOrWhiteSpace(request.ClosingNotes)
            ? $"Atención finalizada para el turno {ticket.Folio}."
            : $"Atención finalizada para el turno {ticket.Folio}. Nota: {request.ClosingNotes.Trim()}";

        AddEvent(
            ticket.Id,
            ticket.ServiceModuleId,
            TicketEventType.ServiceCompleted,
            description,
            request.OperatorName
        );

        await ReleaseModuleIfNeededAsync(ticket.ServiceModuleId);

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Atención finalizada correctamente."
        );
    }

    public async Task<ApiResponse<QueueTicketResponse>> MarkAsNoShowAsync(
        int ticketId,
        MarkQueueTicketNoShowRequest request
    )
    {
        var ticket = await GetTicketForUpdateAsync(ticketId);

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo marcar el turno como no presentado.",
                new[] { "El turno solicitado no existe." }
            );
        }

        if (ticket.Status != QueueTicketStatus.Called)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo marcar el turno como no presentado.",
                new[] { "Solo se puede marcar como no presentado un turno llamado." }
            );
        }

        ticket.Status = QueueTicketStatus.NoShow;
        ticket.NoShowAt = DateTimeOffset.UtcNow;

        var description = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Turno {ticket.Folio} marcado como no presentado."
            : $"Turno {ticket.Folio} marcado como no presentado. Nota: {request.Notes.Trim()}";

        AddEvent(
            ticket.Id,
            ticket.ServiceModuleId,
            TicketEventType.MarkedAsNoShow,
            description,
            request.OperatorName
        );

        await ReleaseModuleIfNeededAsync(ticket.ServiceModuleId);

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Turno marcado como no presentado correctamente."
        );
    }

    public async Task<ApiResponse<QueueTicketResponse>> CancelAsync(int ticketId, CancelQueueTicketRequest request)
    {
        var ticket = await GetTicketForUpdateAsync(ticketId);

        if (ticket is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo cancelar el turno.",
                new[] { "El turno solicitado no existe." }
            );
        }

        if (ticket.Status is QueueTicketStatus.Completed or QueueTicketStatus.Cancelled or QueueTicketStatus.NoShow)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo cancelar el turno.",
                new[] { "El turno ya tiene un estado final y no puede cancelarse." }
            );
        }

        ticket.Status = QueueTicketStatus.Cancelled;
        ticket.CancelledAt = DateTimeOffset.UtcNow;

        AddEvent(
            ticket.Id,
            ticket.ServiceModuleId,
            TicketEventType.Cancelled,
            $"Turno {ticket.Folio} cancelado. Motivo: {request.Reason.Trim()}",
            request.OperatorName
        );

        await ReleaseModuleIfNeededAsync(ticket.ServiceModuleId);

        await _dbContext.SaveChangesAsync();

        var updatedTicket = await GetTicketForResponseAsync(ticket.Id);

        return ApiResponse<QueueTicketResponse>.Ok(
            ToResponse(updatedTicket!),
            "Turno cancelado correctamente."
        );
    }

    private async Task<QueueTicket?> GetTicketForUpdateAsync(int ticketId)
    {
        return await _dbContext.QueueTickets
            .Include(ticket => ticket.Branch)
            .Include(ticket => ticket.ServiceType)
            .Include(ticket => ticket.ServiceModule)
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId);
    }

    private async Task<QueueTicket?> GetTicketForResponseAsync(int ticketId)
    {
        return await _dbContext.QueueTickets
            .AsNoTracking()
            .Include(ticket => ticket.Branch)
            .Include(ticket => ticket.ServiceType)
            .Include(ticket => ticket.ServiceModule)
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId);
    }

    private void AddEvent(
        int queueTicketId,
        int? serviceModuleId,
        TicketEventType eventType,
        string description,
        string? operatorName
    )
    {
        var ticketEvent = new TicketEvent
        {
            QueueTicketId = queueTicketId,
            ServiceModuleId = serviceModuleId,
            EventType = eventType,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = NormalizeOperatorName(operatorName)
        };

        _dbContext.TicketEvents.Add(ticketEvent);
    }

    private async Task ReleaseModuleIfNeededAsync(int? serviceModuleId)
    {
        if (!serviceModuleId.HasValue)
        {
            return;
        }

        var module = await _dbContext.ServiceModules
            .FirstOrDefaultAsync(item => item.Id == serviceModuleId.Value);

        if (module is null)
        {
            return;
        }

        module.Status = ServiceModuleStatus.Active;
        module.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static QueueTicketResponse ToResponse(QueueTicket ticket)
    {
        return new QueueTicketResponse(
            ticket.Id,
            ticket.BranchId,
            ticket.Branch?.Name ?? "Sucursal no disponible",
            ticket.ServiceTypeId,
            ticket.ServiceType?.Name ?? "Servicio no disponible",
            ticket.ServiceType?.Prefix ?? string.Empty,
            ticket.ServiceModuleId,
            ticket.ServiceModule?.Name,
            ticket.Folio,
            ticket.DailySequence,
            ticket.Status,
            ticket.CustomerName,
            ticket.CustomerReference,
            ticket.Notes,
            ticket.ServiceDate,
            ticket.CreatedAt,
            ticket.CalledAt,
            ticket.ServiceStartedAt,
            ticket.ServiceCompletedAt,
            ticket.CancelledAt,
            ticket.NoShowAt
        );
    }

    private static string NormalizeOperatorName(string? operatorName)
    {
        if (string.IsNullOrWhiteSpace(operatorName))
        {
            return "Sistema";
        }

        return operatorName.Trim();
    }
}
