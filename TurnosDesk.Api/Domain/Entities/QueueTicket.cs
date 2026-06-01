using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class QueueTicket
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public Branch? Branch { get; set; }

    public int ServiceTypeId { get; set; }

    public ServiceType? ServiceType { get; set; }

    public int? ServiceModuleId { get; set; }

    public ServiceModule? ServiceModule { get; set; }

    public string Folio { get; set; } = string.Empty;

    public int DailySequence { get; set; }

    public QueueTicketStatus Status { get; set; } = QueueTicketStatus.Pending;

    public string? CustomerName { get; set; }

    public string? CustomerReference { get; set; }

    public string? Notes { get; set; }

    public DateOnly ServiceDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CalledAt { get; set; }

    public DateTimeOffset? ServiceStartedAt { get; set; }

    public DateTimeOffset? ServiceCompletedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public DateTimeOffset? NoShowAt { get; set; }

    public ICollection<TicketEvent> Events { get; set; } = new List<TicketEvent>();
}
