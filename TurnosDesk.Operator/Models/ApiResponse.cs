using System.Text.Json.Serialization;

namespace TurnosDesk.Operator.Models;

public sealed class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public IReadOnlyCollection<string> Errors { get; set; } = Array.Empty<string>();
}
