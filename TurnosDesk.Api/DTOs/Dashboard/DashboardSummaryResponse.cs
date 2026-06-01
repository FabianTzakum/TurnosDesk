namespace TurnosDesk.Api.DTOs.Dashboard;

public sealed record DashboardSummaryResponse(
    DateOnly ServiceDate,
    int TotalTickets,
    int PendingTickets,
    int CalledTickets,
    int InServiceTickets,
    int CompletedTickets,
    int CancelledTickets,
    int NoShowTickets,
    double AverageWaitingMinutes,
    double AverageServiceMinutes,
    IReadOnlyCollection<ServiceTypeMetricResponse> TopServiceTypes,
    IReadOnlyCollection<ServiceModuleStatusMetricResponse> ServiceModulesByStatus,
    IReadOnlyCollection<LatestCalledTicketResponse> LatestCalledTickets
);
