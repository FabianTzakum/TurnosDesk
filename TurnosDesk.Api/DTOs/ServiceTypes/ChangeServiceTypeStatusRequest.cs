using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceTypes;

public sealed class ChangeServiceTypeStatusRequest
{
    [Required(ErrorMessage = "El estado del servicio es obligatorio.")]
    public ServiceTypeStatus Status { get; set; } = ServiceTypeStatus.Active;
}
