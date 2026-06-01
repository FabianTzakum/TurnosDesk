using System.ComponentModel.DataAnnotations;

namespace TurnosDesk.Api.DTOs.Branches;

public sealed class CreateBranchRequest
{
    [Required(ErrorMessage = "El código de la sucursal es obligatorio.")]
    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La dirección no puede superar los 250 caracteres.")]
    public string? Address { get; set; }

    [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
    public string? PhoneNumber { get; set; }
}
