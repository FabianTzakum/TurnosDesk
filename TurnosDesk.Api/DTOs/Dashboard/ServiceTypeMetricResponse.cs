namespace TurnosDesk.Api.DTOs.Dashboard;

public sealed record ServiceTypeMetricResponse(
    int ServiceTypeId,
    string ServiceTypeName,
    string Prefix,
    int TotalTickets
);
