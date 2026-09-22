// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories;

/// <summary>
/// Staff-only list of every collection category on the platform.
/// Access is granted by the "StaffOnly" policy applied to this whole folder in Program.cs.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ApiClient _apiClient;

    public IndexModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public List<CategoryDto> Categories { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            Categories = await _apiClient.GetCategoriesAsync();
        }
        catch (ApiClientException ex)
        {
            // Show a friendly banner rather than letting the page throw.
            ErrorMessage = ex.Message;
        }
    }
}
