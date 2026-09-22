// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories;

public class DetailsModel : PageModel
{
    private readonly ApiClient _apiClient;

    public DetailsModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public CategoryDto? Category { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        try
        {
            Category = await _apiClient.GetCategoryAsync(code);
        }
        catch (ApiClientException ex)
        {
            // A missing category sends the user back to the list with an explanation,
            // rather than leaving them on an empty details page.
            if (ex.IsNotFound)
            {
                TempData["ErrorMessage"] = $"Category '{code}' no longer exists.";
                return RedirectToPage("./Index");
            }

            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
