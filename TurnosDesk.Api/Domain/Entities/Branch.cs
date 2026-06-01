using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class Branch
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public BranchStatus Status { get; set; } = BranchStatus.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ServiceArea> ServiceAreas { get; set; } = new List<ServiceArea>();

    public ICollection<ServiceModule> ServiceModules { get; set; } = new List<ServiceModule>();

    public ICollection<QueueTicket> QueueTickets { get; set; } = new List<QueueTicket>();
}
