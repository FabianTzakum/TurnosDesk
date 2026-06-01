using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.QueueTickets;

public sealed class CreateQueueTicketRequest
{
    [Required(ErrorMessage = "La sucursal es obligatoria.")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "El servicio es obligatorio.")]
    public int ServiceTypeId { get; set; }

    [StringLength(150, ErrorMessage = "El nombre del cliente no puede superar los 150 caracteres.")]
    public string? CustomerName { get; set; }

    [StringLength(100, ErrorMessage = "La referencia del cliente no puede superar los 100 caracteres.")]
    public string? CustomerReference { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
    public string? Notes { get; set; }
}
