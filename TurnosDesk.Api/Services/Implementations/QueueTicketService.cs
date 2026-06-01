using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.QueueTickets;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class QueueTicketService : IQueueTicketService
{
    private readonly TurnosDeskDbContext _dbContext;

    public QueueTicketService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<QueueTicketResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        int? serviceTypeId,
        QueueTicketStatus? status,
        DateOnly? serviceDate
    )
    {
        var query = _dbContext.QueueTickets
            .AsNoTracking()
            .Include(ticket => ticket.Branch)
            .Include(ticket => ticket.ServiceType)
            .Include(ticket => ticket.ServiceModule)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(ticket => ticket.BranchId == branchId.Value);
        }

        if (serviceTypeId.HasValue)
        {
            query = query.Where(ticket => ticket.ServiceTypeId == serviceTypeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(ticket => ticket.Status == status.Value);
        }

        if (serviceDate.HasValue)
        {
            query = query.Where(ticket => ticket.ServiceDate == serviceDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(ticket =>
                ticket.Folio.Contains(search) ||
                (ticket.CustomerName != null && ticket.CustomerName.Contains(search)) ||
                (ticket.CustomerReference != null && ticket.CustomerReference.Contains(search)) ||
                (ticket.Notes != null && ticket.Notes.Contains(search)) ||
                (ticket.Branch != null && ticket.Branch.Name.Contains(search)) ||
                (ticket.ServiceType != null && ticket.ServiceType.Name.Contains(search)) ||
                (ticket.ServiceModule != null && ticket.ServiceModule.Name.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(ticket => ticket.CreatedAt)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(ticket => ToResponse(ticket))
            .ToListAsync();

        return PagedResponse<QueueTicketResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<QueueTicketResponse?> GetByIdAsync(int id)
    {
        var ticket = await _dbContext.QueueTickets
            .AsNoTracking()
            .Include(item => item.Branch)
            .Include(item => item.ServiceType)
            .Include(item => item.ServiceModule)
            .FirstOrDefaultAsync(item => item.Id == id);

        return ticket is null ? null : ToResponse(ticket);
    }

    public async Task<ApiResponse<QueueTicketResponse>> CreateAsync(CreateQueueTicketRequest request)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(item => item.Id == request.BranchId);

        if (branch is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo generar el turno.",
                new[] { "La sucursal indicada no existe." }
            );
        }

        if (branch.Status != BranchStatus.Active)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo generar el turno.",
                new[] { "La sucursal indicada no está activa." }
            );
        }

        var serviceType = await _dbContext.ServiceTypes
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == request.ServiceTypeId);

        if (serviceType is null)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo generar el turno.",
                new[] { "El servicio indicado no existe." }
            );
        }

        if (serviceType.Status != ServiceTypeStatus.Active)
        {
            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo generar el turno.",
                new[] { "El servicio indicado no está activo." }
            );
        }

        if (serviceType.ServiceAreaId.HasValue)
        {
            var serviceAreaBelongsToBranch = await _dbContext.ServiceAreas
                .AnyAsync(area =>
                    area.Id == serviceType.ServiceAreaId.Value &&
                    area.BranchId == request.BranchId &&
                    area.Status == ServiceAreaStatus.Active
                );

            if (!serviceAreaBelongsToBranch)
            {
                return ApiResponse<QueueTicketResponse>.Fail(
                    "No se pudo generar el turno.",
                    new[] { "El servicio indicado no pertenece a una área activa de la sucursal seleccionada." }
                );
            }
        }

        await using IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var lastSequence = await _dbContext.QueueTickets
                .Where(ticket =>
                    ticket.BranchId == request.BranchId &&
                    ticket.ServiceDate == today
                )
                .MaxAsync(ticket => (int?)ticket.DailySequence) ?? 0;

            var nextSequence = lastSequence + 1;
            var folio = BuildFolio(serviceType.Prefix, nextSequence);

            var ticket = new QueueTicket
            {
                BranchId = request.BranchId,
                ServiceTypeId = request.ServiceTypeId,
                Folio = folio,
                DailySequence = nextSequence,
                Status = QueueTicketStatus.Pending,
                CustomerName = NormalizeOptionalText(request.CustomerName),
                CustomerReference = NormalizeOptionalText(request.CustomerReference),
                Notes = NormalizeOptionalText(request.Notes),
                ServiceDate = today,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _dbContext.QueueTickets.Add(ticket);
            await _dbContext.SaveChangesAsync();

            var ticketEvent = new TicketEvent
            {
                QueueTicketId = ticket.Id,
                EventType = TicketEventType.Created,
                Description = $"Turno {ticket.Folio} generado correctamente.",
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = "Sistema"
            };

            _dbContext.TicketEvents.Add(ticketEvent);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            var createdTicket = await _dbContext.QueueTickets
                .AsNoTracking()
                .Include(item => item.Branch)
                .Include(item => item.ServiceType)
                .Include(item => item.ServiceModule)
                .FirstAsync(item => item.Id == ticket.Id);

            return ApiResponse<QueueTicketResponse>.Ok(
                ToResponse(createdTicket),
                "Turno generado correctamente."
            );
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            return ApiResponse<QueueTicketResponse>.Fail(
                "No se pudo generar el turno.",
                new[] { "Ocurrió un conflicto al generar el folio. Intenta nuevamente." }
            );
        }
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

    private static string BuildFolio(string prefix, int sequence)
    {
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
            ? "T"
            : prefix.Trim().ToUpperInvariant();

        return $"{normalizedPrefix}{sequence:000}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
