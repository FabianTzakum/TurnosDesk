using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.ServiceAreas;

public sealed class ChangeServiceAreaStatusRequest
{
    [Required(ErrorMessage = "El estado del área de atención es obligatorio.")]
    public ServiceAreaStatus Status { get; set; } = ServiceAreaStatus.Active;
}
