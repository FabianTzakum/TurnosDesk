using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceModules;

public sealed class UpdateServiceModuleRequest
{
    [Required(ErrorMessage = "La sucursal es obligatoria.")]
    public int BranchId { get; set; }

    public int? ServiceAreaId { get; set; }

    [Required(ErrorMessage = "El código del módulo es obligatorio.")]
    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del módulo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    public ServiceModuleType Type { get; set; } = ServiceModuleType.Window;

    public ServiceModuleStatus Status { get; set; } = ServiceModuleStatus.Active;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Description { get; set; }
}
