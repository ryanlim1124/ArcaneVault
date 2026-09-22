// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems;

public class EditModel : PageModel
{
    private readonly ApiClient _apiClient;

    public EditModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public int ItemId { get; set; }

    [BindProperty]
    public CollectionItemInputDto Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = [];

    private async Task LoadCategoryOptionsAsync()
    {
        var categories = await _apiClient.GetCategoriesAsync();
        CategoryOptions = categories
            .Select(c => new SelectListItem($"{c.CategoryName} ({c.CategoryCode})", c.CategoryCode))
            .ToList();
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            var item = await _apiClient.GetCollectionItemAsync(id);

            // Ownership check: a collector may only edit items in their own collection,
            // even if they type another item's id straight into the URL.
            if (!string.Equals(item.UserName, User.Identity!.Name, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "You can only edit items in your own collection.";
                return RedirectToPage("./Index");
            }

            ItemId = item.ItemId;
            Input = new CollectionItemInputDto
            {
                ItemName = item.ItemName,
                StartingQuantity = item.StartingQuantity,
                CurrentQuantity = item.CurrentQuantity,
                UserName = item.UserName,
                CategoryCodes = item.CategoryCodes,
            };

            await LoadCategoryOptionsAsync();
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
        Input.UserName = User.Identity!.Name!;

        if (Input.CurrentQuantity > Input.StartingQuantity)
        {
            ModelState.AddModelError(
                "Input.CurrentQuantity",
                "Current quantity cannot be greater than starting quantity.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoryOptionsAsync();
            return Page();
        }

        try
        {
            await _apiClient.UpdateCollectionItemAsync(ItemId, Input);
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategoryOptionsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = $"'{Input.ItemName}' was updated.";
        return RedirectToPage("./Index");
    }
}
