using System.Text.Json.Serialization;

namespace TurnosDesk.Operator.Models;

public sealed class QueueTicketResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("branchId")]
    public int BranchId { get; set; }

    [JsonPropertyName("branchName")]
    public string BranchName { get; set; } = string.Empty;

    [JsonPropertyName("serviceTypeId")]
    public int ServiceTypeId { get; set; }

    [JsonPropertyName("serviceTypeName")]
    public string ServiceTypeName { get; set; } = string.Empty;

    [JsonPropertyName("serviceTypePrefix")]
    public string ServiceTypePrefix { get; set; } = string.Empty;

    [JsonPropertyName("serviceModuleId")]
    public int? ServiceModuleId { get; set; }

    [JsonPropertyName("serviceModuleName")]
    public string? ServiceModuleName { get; set; }

    [JsonPropertyName("folio")]
    public string Folio { get; set; } = string.Empty;

    [JsonPropertyName("dailySequence")]
    public int DailySequence { get; set; }

    [JsonPropertyName("status")]
    public QueueTicketStatus Status { get; set; }
}
