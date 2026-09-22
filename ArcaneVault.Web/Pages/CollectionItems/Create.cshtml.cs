// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems;

public class CreateModel : PageModel
{
    private readonly ApiClient _apiClient;

    public CreateModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CollectionItemInputDto Input { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = [];
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Loads the category checkboxes. Called on GET, and again whenever a POST fails
    /// validation - the options are not part of the posted form, so they must be rebuilt
    /// before the page is redisplayed.
    /// </summary>
    private async Task LoadCategoryOptionsAsync()
    {
        var categories = await _apiClient.GetCategoriesAsync();
        CategoryOptions = categories
            .Select(c => new SelectListItem($"{c.CategoryName} ({c.CategoryCode})", c.CategoryCode))
            .ToList();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadCategoryOptionsAsync();
        }
        catch (ApiClientException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // The owner always comes from the authentication cookie, never from the form, so a
        // collector cannot create an item inside somebody else's collection.
        Input.UserName = User.Identity!.Name!;

        // A rule DataAnnotations cannot express on a single property.
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
            await _apiClient.CreateCollectionItemAsync(Input);
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCategoryOptionsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = $"'{Input.ItemName}' was added to your collection.";
        return RedirectToPage("./Index");
    }
}
