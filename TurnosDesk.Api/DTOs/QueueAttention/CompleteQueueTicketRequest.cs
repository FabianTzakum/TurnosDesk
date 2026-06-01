using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.QueueAttention;

public sealed class CompleteQueueTicketRequest
{
    [StringLength(300, ErrorMessage = "La nota de cierre no puede superar los 300 caracteres.")]
    public string? ClosingNotes { get; set; }

    [StringLength(100, ErrorMessage = "El usuario operador no puede superar los 100 caracteres.")]
    public string? OperatorName { get; set; }
}
