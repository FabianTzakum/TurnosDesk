using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.QueueAttention;

public sealed class RecallQueueTicketRequest
{
    [Required(ErrorMessage = "El módulo de atención es obligatorio.")]
    public int ServiceModuleId { get; set; }

    [StringLength(100, ErrorMessage = "El usuario operador no puede superar los 100 caracteres.")]
    public string? OperatorName { get; set; }
}
