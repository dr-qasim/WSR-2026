using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PlantProduction.Desktop.Laboratory;

public sealed class DesktopApiClient
{
    private HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string BaseUrl { get; private set; } = "http://localhost:5114/";
    public string? Token { get; private set; }

    public void SetBaseUrl(string baseUrl)
    {
        BaseUrl = baseUrl.Trim().TrimEnd('/') + "/";
        var savedToken = Token;

        _httpClient.Dispose();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        if (!string.IsNullOrWhiteSpace(savedToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
        }
    }

    public void SetToken(string? token)
    {
        Token = token;
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<LoginResponse> LoginAsync(string login, string password)
    {
        var response = await PostAsync<LoginRequest, LoginResponse>("api/auth/login", new LoginRequest
        {
            Login = login,
            Password = password
        });

        SetToken(response.Token);
        return response;
    }

    public Task<List<RawMaterialLotListItem>> GetRawMaterialLotsAsync() => GetAsync<List<RawMaterialLotListItem>>("api/catalog/raw-material-lots");
    public Task<List<QualitySpecificationItem>> GetSpecificationsAsync() => GetAsync<List<QualitySpecificationItem>>("api/laboratory/specifications");
    public Task<List<LaboratoryTestItem>> GetTestsAsync() => GetAsync<List<LaboratoryTestItem>>("api/laboratory/tests");
    public Task<List<QualityDecisionItem>> GetDecisionsAsync() => GetAsync<List<QualityDecisionItem>>("api/laboratory/decisions");
    public Task<LaboratoryTestDetail> GetTestAsync(int id) => GetAsync<LaboratoryTestDetail>($"api/laboratory/tests/{id}");
    public Task CreateTestAsync(CreateLaboratoryTestRequest request) => PostWithoutDataAsync("api/laboratory/tests", request);
    public Task StartTestAsync(int testId, int testerUserId) => PostWithoutDataAsync($"api/laboratory/tests/{testId}/start", new StartLaboratoryTestRequest { TesterUserId = testerUserId });
    public Task SaveResultsAsync(int testId, SaveLaboratoryResultsRequest request) => PostWithoutDataAsync($"api/laboratory/tests/{testId}/results", request);
    public Task CompleteTestAsync(int testId, CompleteLaboratoryTestRequest request) => PostWithoutDataAsync($"api/laboratory/tests/{testId}/complete", request);
    public Task CreateDecisionAsync(CreateQualityDecisionRequest request) => PostWithoutDataAsync("api/laboratory/decisions", request);

    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        var payload = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(payload, _jsonOptions);

        if (!response.IsSuccessStatusCode || envelope is null || !envelope.Success || envelope.Data is null)
        {
            throw new InvalidOperationException(envelope?.Message ?? payload);
        }

        return envelope.Data;
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var content = BuildJsonContent(request);
        var response = await _httpClient.PostAsync(url, content);
        var payload = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<TResponse>>(payload, _jsonOptions);

        if (!response.IsSuccessStatusCode || envelope is null || !envelope.Success || envelope.Data is null)
        {
            throw new InvalidOperationException(envelope?.Message ?? payload);
        }

        return envelope.Data;
    }

    private async Task PostWithoutDataAsync<TRequest>(string url, TRequest request)
    {
        var content = BuildJsonContent(request);
        var response = await _httpClient.PostAsync(url, content);
        var payload = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope>(payload, _jsonOptions);

        if (!response.IsSuccessStatusCode || envelope is null || !envelope.Success)
        {
            throw new InvalidOperationException(envelope?.Message ?? payload);
        }
    }

    private StringContent BuildJsonContent<TRequest>(TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
