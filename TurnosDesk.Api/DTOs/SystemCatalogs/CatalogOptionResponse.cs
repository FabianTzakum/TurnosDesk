namespace TurnosDesk.Api.DTOs.SystemCatalogs;

public sealed record CatalogOptionResponse(
    string Value,
    string Label,
    int NumericValue
);
