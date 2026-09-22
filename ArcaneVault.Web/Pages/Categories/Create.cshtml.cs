// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories;

public class CreateModel : PageModel
{
    private readonly ApiClient _apiClient;

    public CreateModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public CategoryInputDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _apiClient.CreateCategoryAsync(Input);
        }
        catch (ApiClientException ex)
        {
            // e.g. "A category with the code 'TCG' already exists." - shown above the form.
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        TempData["SuccessMessage"] = $"Category '{Input.CategoryName}' was created.";
        return RedirectToPage("./Index");
    }
}
