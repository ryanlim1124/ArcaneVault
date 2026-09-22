// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Insights;

/// <summary>
/// VAULT INSIGHTS dashboard - the custom feature's user interface.
///
/// The page renders an empty shell; the numbers arrive afterwards from the OnGet*Async
/// handlers below, which the page's JavaScript fetches. Those handlers call
/// ArcaneVault.Api server-side through ApiClient.
///
/// Why go through page handlers instead of letting the browser call the API directly?
///  - Same-origin: the browser only ever requests /Insights?handler=..., so no CORS setup.
///  - Security: the API's address stays server-side, and these handlers inherit the
///    "StaffOnly" policy applied to this folder, so the data is protected by the same
///    rule as the page itself.
///
/// Staff-only access is configured in Program.cs (AuthorizeFolder("/Insights", "StaffOnly")).
/// </summary>
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApiClient apiClient, ILogger<IndexModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public void OnGet()
    {
    }

    /// <summary>
    /// Runs one of the analytics calls and wraps any failure as a 502 with a readable
    /// message, so the dashboard's JavaScript can show an inline notice on that panel
    /// alone instead of the whole page breaking.
    /// </summary>
    private async Task<JsonResult> SafeJsonAsync<T>(Func<Task<T>> load, string panelName)
    {
        try
        {
            return new JsonResult(await load());
        }
        catch (ApiClientException ex)
        {
            _logger.LogWarning(ex, "Vault Insights: {Panel} failed to load", panelName);
            return new JsonResult(new { error = ex.Message }) { StatusCode = 502 };
        }
    }

    // GET /Insights?handler=Overview
    public Task<JsonResult> OnGetOverviewAsync() =>
        SafeJsonAsync(() => _apiClient.GetVaultOverviewAsync(), "overview");

    // GET /Insights?handler=Demand&top=10
    public Task<JsonResult> OnGetDemandAsync(int top = 10) =>
        SafeJsonAsync(() => _apiClient.GetDemandSignalsAsync(top), "demand");

    // GET /Insights?handler=Categories
    public Task<JsonResult> OnGetCategoriesAsync() =>
        SafeJsonAsync(() => _apiClient.GetCategoryPopularityAsync(), "categories");

    // GET /Insights?handler=Activity&months=6
    public Task<JsonResult> OnGetActivityAsync(int months = 6) =>
        SafeJsonAsync(() => _apiClient.GetActivityTrendAsync(months), "activity");

    // GET /Insights?handler=Highlights
    public Task<JsonResult> OnGetHighlightsAsync() =>
        SafeJsonAsync(() => _apiClient.GetHighlightsAsync(), "highlights");
}
