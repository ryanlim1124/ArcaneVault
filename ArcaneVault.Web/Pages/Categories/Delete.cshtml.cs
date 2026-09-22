// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories;

/// <summary>
/// Delete confirmation. The record is shown first and only removed on POST, so a deletion
/// can never happen from following a link.
/// </summary>
public class DeleteModel : PageModel
{
    private readonly ApiClient _apiClient;

    public DeleteModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public string Code { get; set; } = string.Empty;

    public CategoryDto? Category { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string code)
    {
        try
        {
            Category = await _apiClient.GetCategoryAsync(code);
            Code = Category.CategoryCode;
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
            await _apiClient.DeleteCategoryAsync(Code);
        }
        catch (ApiClientException ex)
        {
            // The API refuses to delete a category still referenced by collection items.
            // Reload the record so the confirmation page can redisplay with the reason.
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Index");
        }

        TempData["SuccessMessage"] = $"Category '{Code}' was deleted.";
        return RedirectToPage("./Index");
    }
}
