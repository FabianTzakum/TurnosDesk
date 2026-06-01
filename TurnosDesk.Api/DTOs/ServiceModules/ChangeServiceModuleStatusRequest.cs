using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceModules;

public sealed class ChangeServiceModuleStatusRequest
{
    [Required(ErrorMessage = "El estado del módulo de atención es obligatorio.")]
    public ServiceModuleStatus Status { get; set; } = ServiceModuleStatus.Active;
}
