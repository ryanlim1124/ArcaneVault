// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArcaneVault.Web.Models;

//It is the only door between my web application and my api, it sends a http get request
//to localhost:5001/api/insights/demand to ask the api for the calculated numbers



namespace ArcaneVault.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

   
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

   

    private async Task<T> GetAsync<T>(string url)
    {
        try
        {
            using var response = await _http.GetAsync(url);
            return await ReadOrThrowAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            throw UnreachableApi(ex, url);
        }
    }

    private async Task<T> PostAsync<T>(string url, object body)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(url, body);
            return await ReadOrThrowAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            throw UnreachableApi(ex, url);
        }
    }

    private async Task PutAsync(string url, object body)
    {
        try
        {
            using var response = await _http.PutAsJsonAsync(url, body);
            await EnsureSuccessAsync(response);
        }
        catch (HttpRequestException ex)
        {
            throw UnreachableApi(ex, url);
        }
    }

    private async Task DeleteAsync(string url)
    {
        try
        {
            using var response = await _http.DeleteAsync(url);
            await EnsureSuccessAsync(response);
        }
        catch (HttpRequestException ex)
        {
            throw UnreachableApi(ex, url);
        }
    }

    private ApiClientException UnreachableApi(HttpRequestException ex, string url)
    {
        _logger.LogError(ex, "Could not reach ArcaneVault.Api at {Url}", url);
        return new ApiClientException("Could not reach the Arcane Vault API. Please make sure the ArcaneVault.Api project is running.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new ApiClientException(await ExtractErrorMessageAsync(response), (int)response.StatusCode);
        }
    }

    private static async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result ?? throw new ApiClientException("The API returned an empty response.");
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (body.ValueKind == JsonValueKind.Object)
            {
                if (body.TryGetProperty("message", out var messageProp))
                {
                    return messageProp.GetString() ?? DefaultMessage(response);
                }

                if (body.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var messages = errorsProp.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .ToList();

                    if (messages.Count > 0)
                    {
                        return string.Join(" ", messages);
                    }
                }
            }
        }
        catch (JsonException)
        {
        }
        catch (NotSupportedException)
        {
        }

        return DefaultMessage(response);
    }

    private static string DefaultMessage(HttpResponseMessage response) => response.StatusCode switch
    {
        HttpStatusCode.NotFound => "The requested item was not found.",
        HttpStatusCode.Unauthorized => "Incorrect username or password.",
        _ => $"The API request failed ({(int)response.StatusCode} {response.ReasonPhrase}).",
    };

    private static string BuildQuery(params (string Key, string? Value)[] parameters)
    {
        var pairs = parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}");

        var query = string.Join("&", pairs);
        return query.Length == 0 ? string.Empty : $"?{query}";
    }

    // ===============================================================
    // Accounts
    // ===============================================================

    public Task<UserDto> RegisterAsync(RegisterInputDto input) =>
        PostAsync<UserDto>("api/accounts/register", new
        {
            input.UserName,
            input.Email,
            input.Password,
        });

    public Task<UserDto> LoginAsync(LoginInputDto input) =>
        PostAsync<UserDto>("api/accounts/login", input);

    // ===============================================================
    // Categories
    // ===============================================================

    public Task<List<CategoryDto>> GetCategoriesAsync() =>
        GetAsync<List<CategoryDto>>("api/categories");

    public Task<CategoryDto> GetCategoryAsync(string categoryCode) =>
        GetAsync<CategoryDto>($"api/categories/{Uri.EscapeDataString(categoryCode)}");

    public Task<CategoryDto> CreateCategoryAsync(CategoryInputDto input) =>
        PostAsync<CategoryDto>("api/categories", input);

    public Task UpdateCategoryAsync(string categoryCode, CategoryInputDto input) =>
        PutAsync($"api/categories/{Uri.EscapeDataString(categoryCode)}", input);

    public Task DeleteCategoryAsync(string categoryCode) =>
        DeleteAsync($"api/categories/{Uri.EscapeDataString(categoryCode)}");

    // ===============================================================
    // Collection items
    // ===============================================================

    public Task<PagedResult<CollectionItemDto>> GetCollectionItemsAsync(
        string? userName = null, string? search = null, int page = 1, int pageSize = 10)
    {
        var query = BuildQuery(
            ("userName", userName),
            ("search", search),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return GetAsync<PagedResult<CollectionItemDto>>($"api/collectionitems{query}");
    }

    public Task<CollectionItemDto> GetCollectionItemAsync(int id) =>
        GetAsync<CollectionItemDto>($"api/collectionitems/{id}");

    public Task<CollectionItemDto> CreateCollectionItemAsync(CollectionItemInputDto input) =>
        PostAsync<CollectionItemDto>("api/collectionitems", input);

    public Task UpdateCollectionItemAsync(int id, CollectionItemInputDto input) =>
        PutAsync($"api/collectionitems/{id}", input);

    public Task DeleteCollectionItemAsync(int id) =>
        DeleteAsync($"api/collectionitems/{id}");

    // ===============================================================
    // Vault Insights (the custom feature)
    // ===============================================================

    public Task<VaultOverviewDto> GetVaultOverviewAsync() =>
        GetAsync<VaultOverviewDto>("api/insights/overview");

    public Task<List<DemandSignalDto>> GetDemandSignalsAsync(int top = 10) =>
        GetAsync<List<DemandSignalDto>>($"api/insights/demand?top={top}");

    public Task<List<CategoryPopularityDto>> GetCategoryPopularityAsync() =>
        GetAsync<List<CategoryPopularityDto>>("api/insights/categories");

    public Task<List<ActivityPointDto>> GetActivityTrendAsync(int months = 6) =>
        GetAsync<List<ActivityPointDto>>($"api/insights/activity?months={months}");

    public Task<List<InsightHighlightDto>> GetHighlightsAsync() =>
        GetAsync<List<InsightHighlightDto>>("api/insights/highlights");
}
