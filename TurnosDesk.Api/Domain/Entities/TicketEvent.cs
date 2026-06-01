using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Domain.Entities;

public class TicketEvent
{
    public int Id { get; set; }

    public int QueueTicketId { get; set; }

    public QueueTicket? QueueTicket { get; set; }

    public int? ServiceModuleId { get; set; }

    public ServiceModule? ServiceModule { get; set; }

    public TicketEventType EventType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }
}
