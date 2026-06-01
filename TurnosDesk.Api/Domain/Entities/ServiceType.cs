using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class ServiceType
{
    public int Id { get; set; }

    public int? ServiceAreaId { get; set; }

    public ServiceArea? ServiceArea { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Prefix { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EstimatedMinutes { get; set; } = 10;

    public ServicePriority Priority { get; set; } = ServicePriority.Normal;

    public ServiceTypeStatus Status { get; set; } = ServiceTypeStatus.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<QueueTicket> QueueTickets { get; set; } = new List<QueueTicket>();
}
