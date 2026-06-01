using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class ServiceModule
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public int? ServiceAreaId { get; set; }

    public ServiceArea? ServiceArea { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ServiceModuleType Type { get; set; } = ServiceModuleType.Window;

    public ServiceModuleStatus Status { get; set; } = ServiceModuleStatus.Active;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<QueueTicket> QueueTickets { get; set; } = new List<QueueTicket>();

    public ICollection<TicketEvent> TicketEvents { get; set; } = new List<TicketEvent>();
}
