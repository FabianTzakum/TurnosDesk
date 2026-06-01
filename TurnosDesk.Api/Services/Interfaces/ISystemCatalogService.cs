using TurnosDesk.Api.DTOs.SystemCatalogs;

namespace TurnosDesk.Api.Services.Interfaces;

public interface ISystemCatalogService
{
    SystemCatalogsResponse GetCatalogs();
}
