using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.QueueAttention;

public sealed class CancelQueueTicketRequest
{
    [Required(ErrorMessage = "El motivo de cancelación es obligatorio.")]
    [StringLength(300, ErrorMessage = "El motivo de cancelación no puede superar los 300 caracteres.")]
    public string Reason { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El usuario operador no puede superar los 100 caracteres.")]
    public string? OperatorName { get; set; }
}
