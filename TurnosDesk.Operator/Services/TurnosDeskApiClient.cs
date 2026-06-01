using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TurnosDesk.Operator.Models;

namespace TurnosDesk.Operator.Services;

public sealed class TurnosDeskApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TurnosDeskApiClient(string apiBaseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/")
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<IReadOnlyCollection<BranchResponse>> GetBranchesAsync()
    {
        var response = await GetAsync<PagedResponse<BranchResponse>>("api/branches?page=1&pageSize=100");
        return response.Items;
    }

    public async Task<IReadOnlyCollection<ServiceModuleResponse>> GetModulesAsync(int branchId)
    {
        var response = await GetAsync<PagedResponse<ServiceModuleResponse>>(
            $"api/service-modules?branchId={branchId}&status=Active&page=1&pageSize=100"
        );

        return response.Items;
    }

    public async Task<IReadOnlyCollection<ServiceTypeResponse>> GetServiceTypesAsync()
    {
        var response = await GetAsync<PagedResponse<ServiceTypeResponse>>(
            "api/service-types?status=Active&page=1&pageSize=100"
        );

        return response.Items;
    }

    public async Task<QueueTicketResponse> CallNextAsync(
        int branchId,
        int serviceModuleId,
        int? serviceTypeId,
        string operatorName
    )
    {
        var body = new
        {
            branchId,
            serviceModuleId,
            serviceTypeId,
            operatorName
        };

        return await PostAsync<QueueTicketResponse>("api/queue-attention/call-next", body);
    }

    public async Task<QueueTicketResponse> StartServiceAsync(int ticketId, string operatorName)
    {
        var body = new
        {
            operatorName
        };

        return await PostAsync<QueueTicketResponse>($"api/queue-attention/{ticketId}/start", body);
    }

    public async Task<QueueTicketResponse> CompleteServiceAsync(
        int ticketId,
        string closingNotes,
        string operatorName
    )
    {
        var body = new
        {
            closingNotes,
            operatorName
        };

        return await PostAsync<QueueTicketResponse>($"api/queue-attention/{ticketId}/complete", body);
    }

    public async Task<QueueTicketResponse> MarkNoShowAsync(
        int ticketId,
        string notes,
        string operatorName
    )
    {
        var body = new
        {
            notes,
            operatorName
        };

        return await PostAsync<QueueTicketResponse>($"api/queue-attention/{ticketId}/no-show", body);
    }

    public async Task<QueueTicketResponse> CancelAsync(
        int ticketId,
        string reason,
        string operatorName
    )
    {
        var body = new
        {
            reason,
            operatorName
        };

        return await PostAsync<QueueTicketResponse>($"api/queue-attention/{ticketId}/cancel", body);
    }

    private async Task<T> GetAsync<T>(string url)
    {
        using var response = await _httpClient.GetAsync(url);
        return await ReadApiResponseAsync<T>(response);
    }

    private async Task<T> PostAsync<T>(string url, object body)
    {
        var json = JsonSerializer.Serialize(body, _jsonOptions);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClient.PostAsync(url, content);
        return await ReadApiResponseAsync<T>(response);
    }

    private async Task<T> ReadApiResponseAsync<T>(HttpResponseMessage httpResponse)
    {
        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<T>>(_jsonOptions);

        if (apiResponse is null)
        {
            throw new InvalidOperationException("El API no devolvió una respuesta válida.");
        }

        if (!apiResponse.Success)
        {
            var errors = apiResponse.Errors.Count == 0
                ? apiResponse.Message
                : string.Join(Environment.NewLine, apiResponse.Errors);

            throw new InvalidOperationException(errors);
        }

        if (apiResponse.Data is null)
        {
            throw new InvalidOperationException("El API no devolvió información para esta operación.");
        }

        return apiResponse.Data;
    }
}
