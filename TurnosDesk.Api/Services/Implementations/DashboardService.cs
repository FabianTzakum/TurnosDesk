using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.Dashboard;
using TurnosDesk.Api.Services.Interfaces;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class DashboardService : IDashboardService
{
    private readonly TurnosDeskDbContext _dbContext;

    public DashboardService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        int? branchId,
        DateOnly? serviceDate
    )
    {
        var targetDate = serviceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var ticketsQuery = _dbContext.QueueTickets
            .AsNoTracking()
            .Include(ticket => ticket.ServiceType)
            .Include(ticket => ticket.ServiceModule)
            .Where(ticket => ticket.ServiceDate == targetDate);

        if (branchId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(ticket => ticket.BranchId == branchId.Value);
        }

        var totalTickets = await ticketsQuery.CountAsync();

        var pendingTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.Pending);
        var calledTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.Called);
        var inServiceTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.InService);
        var completedTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.Completed);
        var cancelledTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.Cancelled);
        var noShowTickets = await ticketsQuery.CountAsync(ticket => ticket.Status == QueueTicketStatus.NoShow);

        var waitingMinutes = await ticketsQuery
            .Where(ticket => ticket.CalledAt.HasValue)
            .Select(ticket => EF.Functions.DateDiffMinute(ticket.CreatedAt, ticket.CalledAt!.Value))
            .ToListAsync();

        var serviceMinutes = await ticketsQuery
            .Where(ticket => ticket.ServiceStartedAt.HasValue && ticket.ServiceCompletedAt.HasValue)
            .Select(ticket => EF.Functions.DateDiffMinute(ticket.ServiceStartedAt!.Value, ticket.ServiceCompletedAt!.Value))
            .ToListAsync();

        var averageWaitingMinutes = waitingMinutes.Count == 0
            ? 0
            : Math.Round(waitingMinutes.Average(), 2);

        var averageServiceMinutes = serviceMinutes.Count == 0
            ? 0
            : Math.Round(serviceMinutes.Average(), 2);

        var topServiceTypes = await ticketsQuery
            .GroupBy(ticket => new
            {
                ticket.ServiceTypeId,
                ServiceTypeName = ticket.ServiceType != null
                    ? ticket.ServiceType.Name
                    : "Servicio no disponible",
                Prefix = ticket.ServiceType != null
                    ? ticket.ServiceType.Prefix
                    : string.Empty
            })
            .Select(group => new ServiceTypeMetricResponse(
                group.Key.ServiceTypeId,
                group.Key.ServiceTypeName,
                group.Key.Prefix,
                group.Count()
            ))
            .OrderByDescending(metric => metric.TotalTickets)
            .ThenBy(metric => metric.ServiceTypeName)
            .Take(5)
            .ToListAsync();

        var modulesQuery = _dbContext.ServiceModules
            .AsNoTracking()
            .AsQueryable();

        if (branchId.HasValue)
        {
            modulesQuery = modulesQuery.Where(module => module.BranchId == branchId.Value);
        }

        var modulesByStatus = await modulesQuery
            .GroupBy(module => module.Status)
            .Select(group => new ServiceModuleStatusMetricResponse(
                group.Key,
                group.Count()
            ))
            .OrderBy(metric => metric.Status)
            .ToListAsync();

        var latestCalledTickets = await ticketsQuery
            .Where(ticket => ticket.CalledAt.HasValue)
            .OrderByDescending(ticket => ticket.CalledAt)
            .Take(8)
            .Select(ticket => new LatestCalledTicketResponse(
                ticket.Id,
                ticket.Folio,
                ticket.Status,
                ticket.ServiceType != null
                    ? ticket.ServiceType.Name
                    : "Servicio no disponible",
                ticket.ServiceModule != null
                    ? ticket.ServiceModule.Name
                    : null,
                ticket.CalledAt
            ))
            .ToListAsync();

        return new DashboardSummaryResponse(
            targetDate,
            totalTickets,
            pendingTickets,
            calledTickets,
            inServiceTickets,
            completedTickets,
            cancelledTickets,
            noShowTickets,
            averageWaitingMinutes,
            averageServiceMinutes,
            topServiceTypes,
            modulesByStatus,
            latestCalledTickets
        );
    }
}
