using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PlantProduction.Desktop;

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
        var token = Token;

        _httpClient.Dispose();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

    public Task<List<ProductListItem>> GetProductsAsync() => GetAsync<List<ProductListItem>>("api/catalog/products");
    public Task<ProductDetail> GetProductAsync(int id) => GetAsync<ProductDetail>($"api/catalog/products/{id}");
    public Task<List<RawMaterialListItem>> GetRawMaterialsAsync() => GetAsync<List<RawMaterialListItem>>("api/catalog/raw-materials");
    public Task<List<RawMaterialLotListItem>> GetRawMaterialLotsAsync() => GetAsync<List<RawMaterialLotListItem>>("api/catalog/raw-material-lots");
    public Task<List<ProductionLineItem>> GetProductionLinesAsync() => GetAsync<List<ProductionLineItem>>("api/catalog/production-lines");

    public Task<List<RecipeListItem>> GetRecipesAsync() => GetAsync<List<RecipeListItem>>("api/recipes");
    public Task<RecipeDetail> GetRecipeAsync(int id) => GetAsync<RecipeDetail>($"api/recipes/{id}");
    public Task CreateRecipeAsync(CreateRecipeRequest request) => PostWithoutDataAsync("api/recipes", request);
    public Task ApproveRecipeAsync(int id, int approvedByUserId, string? comment) => PostWithoutDataAsync($"api/recipes/{id}/approve", new ApproveRecipeRequest
    {
        ApprovedByUserId = approvedByUserId,
        Comment = comment
    });

    public Task<List<TechnologyCardListItem>> GetTechnologyCardsAsync() => GetAsync<List<TechnologyCardListItem>>("api/technology-cards");
    public Task<TechnologyCardDetail> GetTechnologyCardAsync(int id) => GetAsync<TechnologyCardDetail>($"api/technology-cards/{id}");
    public Task CreateTechnologyCardAsync(CreateTechnologyCardRequest request) => PostWithoutDataAsync("api/technology-cards", request);
    public Task ApproveTechnologyCardAsync(int id, int approvedByUserId, string? comment) => PostWithoutDataAsync($"api/technology-cards/{id}/approve", new ApproveTechnologyCardRequest
    {
        ApprovedByUserId = approvedByUserId,
        Comment = comment
    });

    public Task<List<ProductionOrderItem>> GetOrdersAsync() => GetAsync<List<ProductionOrderItem>>("api/production/orders");
    public Task CreateOrderAsync(CreateProductionOrderRequest request) => PostWithoutDataAsync("api/production/orders", request);
    public Task<List<ProductionBatchItem>> GetBatchesAsync() => GetAsync<List<ProductionBatchItem>>("api/production/batches");
    public Task CreateBatchAsync(CreateProductionBatchRequest request) => PostWithoutDataAsync("api/production/batches", request);
    public Task<List<BatchStepRunItem>> GetBatchStepsAsync(int batchId) => GetAsync<List<BatchStepRunItem>>($"api/production/batches/{batchId}/steps");
    public Task StartBatchAsync(int batchId) => PostWithoutDataAsync($"api/production/batches/{batchId}/start", new { });
    public Task CompleteBatchAsync(int batchId) => PostWithoutDataAsync($"api/production/batches/{batchId}/complete", new { });
    public Task StartStepAsync(int stepRunId, StartStepRunRequest request) => PostWithoutDataAsync($"api/production/step-runs/{stepRunId}/start", request);
    public Task CompleteStepAsync(int stepRunId, CompleteStepRunRequest request) => PostWithoutDataAsync($"api/production/step-runs/{stepRunId}/complete", request);
    public Task<MeasurementResponse> AddMeasurementAsync(int stepRunId, AddMeasurementRequest request) => PostAsync<AddMeasurementRequest, MeasurementResponse>($"api/production/step-runs/{stepRunId}/measurements", request);
    public Task CreateDeviationAsync(CreateDeviationRequest request) => PostWithoutDataAsync("api/production/deviations", request);
    public Task<List<DeviationItem>> GetDeviationsAsync() => GetAsync<List<DeviationItem>>("api/production/deviations");

    public Task<List<QualitySpecificationItem>> GetSpecificationsAsync() => GetAsync<List<QualitySpecificationItem>>("api/laboratory/specifications");
    public Task<List<LaboratoryTestItem>> GetTestsAsync() => GetAsync<List<LaboratoryTestItem>>("api/laboratory/tests");
    public Task<List<QualityDecisionItem>> GetDecisionsAsync() => GetAsync<List<QualityDecisionItem>>("api/laboratory/decisions");
    public Task<LaboratoryTestDetail> GetTestAsync(int id) => GetAsync<LaboratoryTestDetail>($"api/laboratory/tests/{id}");
    public Task CreateTestAsync(CreateLaboratoryTestRequest request) => PostWithoutDataAsync("api/laboratory/tests", request);
    public Task StartTestAsync(int testId, int testerUserId) => PostWithoutDataAsync($"api/laboratory/tests/{testId}/start", new StartLaboratoryTestRequest
    {
        TesterUserId = testerUserId
    });
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
