// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems;

/// <summary>
/// Removal confirmation. The API performs a soft delete (IsDeleted = true), so the record
/// disappears from the collector's view while the historical data behind Vault Insights
/// stays intact.
/// </summary>
public class DeleteModel : PageModel
{
    private readonly ApiClient _apiClient;

    public DeleteModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public int ItemId { get; set; }

    public CollectionItemDto? Item { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var item = await _apiClient.GetCollectionItemAsync(id);

            if (!string.Equals(item.UserName, User.Identity!.Name, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "You can only remove items from your own collection.";
                return RedirectToPage("./Index");
            }

            Item = item;
            ItemId = item.ItemId;
        }
        catch (ApiClientException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _apiClient.DeleteCollectionItemAsync(ItemId);
        }
        catch (ApiClientException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Index");
        }

        TempData["SuccessMessage"] = "The item was removed from your collection.";
        return RedirectToPage("./Index");
    }
}
