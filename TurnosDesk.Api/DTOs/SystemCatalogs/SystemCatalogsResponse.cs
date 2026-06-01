namespace TurnosDesk.Api.DTOs.SystemCatalogs;

public sealed record SystemCatalogsResponse(
    IReadOnlyCollection<CatalogOptionResponse> BranchStatuses,
    IReadOnlyCollection<CatalogOptionResponse> ServiceAreaStatuses,
    IReadOnlyCollection<CatalogOptionResponse> ServiceModuleStatuses,
    IReadOnlyCollection<CatalogOptionResponse> ServiceModuleTypes,
    IReadOnlyCollection<CatalogOptionResponse> ServiceTypeStatuses,
    IReadOnlyCollection<CatalogOptionResponse> ServicePriorities,
    IReadOnlyCollection<CatalogOptionResponse> QueueTicketStatuses,
    IReadOnlyCollection<CatalogOptionResponse> TicketEventTypes
);
