using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.QueueAttention;

public sealed class MarkQueueTicketNoShowRequest
{
    [StringLength(300, ErrorMessage = "La observación no puede superar los 300 caracteres.")]
    public string? Notes { get; set; }

    [StringLength(100, ErrorMessage = "El usuario operador no puede superar los 100 caracteres.")]
    public string? OperatorName { get; set; }
}
