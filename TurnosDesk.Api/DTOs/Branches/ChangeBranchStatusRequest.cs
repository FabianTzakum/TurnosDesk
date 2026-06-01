using System.ComponentModel.DataAnnotations;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.DTOs.Branches;

public sealed class ChangeBranchStatusRequest
{
    [Required(ErrorMessage = "El estado de la sucursal es obligatorio.")]
    public BranchStatus Status { get; set; } = BranchStatus.Active;
}
