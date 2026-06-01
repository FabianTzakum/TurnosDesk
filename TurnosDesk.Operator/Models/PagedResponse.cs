using System.Text.Json.Serialization;

namespace TurnosDesk.Operator.Models;

public sealed class PagedResponse<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
