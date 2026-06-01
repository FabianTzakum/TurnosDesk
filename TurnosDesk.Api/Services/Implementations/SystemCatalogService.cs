using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.SystemCatalogs;
using TurnosDesk.Api.Services.Interfaces;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class SystemCatalogService : ISystemCatalogService
{
    public SystemCatalogsResponse GetCatalogs()
    {
        return new SystemCatalogsResponse(
            BranchStatuses: new[]
            {
                CreateOption(BranchStatus.Active, "Activa"),
                CreateOption(BranchStatus.Inactive, "Inactiva")
            },
            ServiceAreaStatuses: new[]
            {
                CreateOption(ServiceAreaStatus.Active, "Activa"),
                CreateOption(ServiceAreaStatus.Inactive, "Inactiva")
            },
            ServiceModuleStatuses: new[]
            {
                CreateOption(ServiceModuleStatus.Active, "Activo"),
                CreateOption(ServiceModuleStatus.Inactive, "Inactivo"),
                CreateOption(ServiceModuleStatus.Busy, "Ocupado"),
                CreateOption(ServiceModuleStatus.Paused, "Pausado"),
                CreateOption(ServiceModuleStatus.Maintenance, "En mantenimiento")
            },
            ServiceModuleTypes: new[]
            {
                CreateOption(ServiceModuleType.Window, "Ventanilla"),
                CreateOption(ServiceModuleType.Cashier, "Caja"),
                CreateOption(ServiceModuleType.Desk, "Escritorio"),
                CreateOption(ServiceModuleType.SupportModule, "Módulo de soporte"),
                CreateOption(ServiceModuleType.Custom, "Personalizado")
            },
            ServiceTypeStatuses: new[]
            {
                CreateOption(ServiceTypeStatus.Active, "Activo"),
                CreateOption(ServiceTypeStatus.Inactive, "Inactivo")
            },
            ServicePriorities: new[]
            {
                CreateOption(ServicePriority.Low, "Baja"),
                CreateOption(ServicePriority.Normal, "Normal"),
                CreateOption(ServicePriority.High, "Alta"),
                CreateOption(ServicePriority.Critical, "Crítica")
            },
            QueueTicketStatuses: new[]
            {
                CreateOption(QueueTicketStatus.Pending, "Pendiente"),
                CreateOption(QueueTicketStatus.Called, "Llamado"),
                CreateOption(QueueTicketStatus.InService, "En atención"),
                CreateOption(QueueTicketStatus.Completed, "Finalizado"),
                CreateOption(QueueTicketStatus.Cancelled, "Cancelado"),
                CreateOption(QueueTicketStatus.NoShow, "No presentado")
            },
            TicketEventTypes: new[]
            {
                CreateOption(TicketEventType.Created, "Turno creado"),
                CreateOption(TicketEventType.Called, "Turno llamado"),
                CreateOption(TicketEventType.ServiceStarted, "Atención iniciada"),
                CreateOption(TicketEventType.ServiceCompleted, "Atención finalizada"),
                CreateOption(TicketEventType.Cancelled, "Turno cancelado"),
                CreateOption(TicketEventType.MarkedAsNoShow, "Marcado como no presentado"),
                CreateOption(TicketEventType.Recalled, "Turno llamado nuevamente")
            }
        );
    }

    private static CatalogOptionResponse CreateOption<TEnum>(TEnum value, string label)
        where TEnum : Enum
    {
        return new CatalogOptionResponse(
            Value: value.ToString(),
            Label: label,
            NumericValue: Convert.ToInt32(value)
        );
    }
}
