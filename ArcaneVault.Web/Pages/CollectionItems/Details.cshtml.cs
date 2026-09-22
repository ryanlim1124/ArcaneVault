// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems;

public class DetailsModel : PageModel
{
    private readonly ApiClient _apiClient;

    public DetailsModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public CollectionItemDto? Item { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var item = await _apiClient.GetCollectionItemAsync(id);

            // Ownership check - see EditModel for the same reasoning.
            if (!string.Equals(item.UserName, User.Identity!.Name, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "You can only view items in your own collection.";
                return RedirectToPage("./Index");
            }

            Item = item;
        }
        catch (ApiClientException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Index");
        }

        return Page();
    }
}
