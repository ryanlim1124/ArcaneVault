// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems;

/// <summary>
/// The signed-in collector's personal collection, with cross-field search and paging.
/// The whole /CollectionItems folder requires authentication (configured in Program.cs),
/// so User.Identity.Name is always populated here.
/// </summary>
public class IndexModel : PageModel
{
    private const int PageSize = 10;

    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// The single search box. Bound from the query string so results stay bookmarkable
    /// and the browser's back button behaves correctly.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<CollectionItemDto> Result { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public int TotalPages => Result.PageSize == 0
        ? 1
        : (int)Math.Ceiling(Result.TotalCount / (double)Result.PageSize);

    public async Task OnGetAsync()
    {
        try
        {
            // Scoped to the signed-in account, taken from the authentication cookie rather
            // than from any value the browser could tamper with - so one collector can
            // never list another's collection.
            Result = await _apiClient.GetCollectionItemsAsync(
                userName: User.Identity!.Name,
                search: Search,
                page: PageNumber < 1 ? 1 : PageNumber,
                pageSize: PageSize);
        }
        catch (ApiClientException ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
