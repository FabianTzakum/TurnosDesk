using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class ServiceArea
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ServiceAreaStatus Status { get; set; } = ServiceAreaStatus.Active;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ServiceModule> ServiceModules { get; set; } = new List<ServiceModule>();

    public ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();
}
