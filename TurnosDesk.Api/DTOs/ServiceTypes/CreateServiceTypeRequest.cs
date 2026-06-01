using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceTypes;

public sealed class CreateServiceTypeRequest
{
    public int? ServiceAreaId { get; set; }

    [Required(ErrorMessage = "El código del servicio es obligatorio.")]
    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El prefijo del servicio es obligatorio.")]
    [StringLength(5, ErrorMessage = "El prefijo no puede superar los 5 caracteres.")]
    public string Prefix { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Description { get; set; }

    [Range(1, 480, ErrorMessage = "El tiempo estimado debe estar entre 1 y 480 minutos.")]
    public int EstimatedMinutes { get; set; } = 10;

    public ServicePriority Priority { get; set; } = ServicePriority.Normal;
}
