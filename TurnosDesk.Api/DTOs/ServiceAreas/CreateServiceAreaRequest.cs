using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.ServiceAreas;

public sealed class CreateServiceAreaRequest
{
    [Required(ErrorMessage = "La sucursal es obligatoria.")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "El código del área es obligatorio.")]
    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del área es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Description { get; set; }
}
