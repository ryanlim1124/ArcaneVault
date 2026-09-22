// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories;

public class EditModel : PageModel
{
    private readonly ApiClient _apiClient;

    public EditModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// The original category code, carried through the form in a hidden field.
    /// It is the primary key, so it identifies which record to update and cannot itself change.
    /// </summary>
    [BindProperty]
    public string Code { get; set; } = string.Empty;

    [BindProperty]
    public CategoryInputDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string code)
    {
        try
        {
            var category = await _apiClient.GetCategoryAsync(code);

            Code = category.CategoryCode;
            Input = new CategoryInputDto
            {
                CategoryCode = category.CategoryCode,
                CategoryName = category.CategoryName,
            };
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
        // The code is shown in a disabled input because it is the primary key and cannot
        // change. Browsers do not submit disabled fields, so model binding just recorded a
        // "Category code is required" error against a field the user cannot even see.
        // Re-assert the value from the hidden Code field, then clear that stale error -
        // without the Remove, ModelState stays invalid and the page silently redisplays
        // with a validation failure that is never rendered anywhere.
        Input.CategoryCode = Code;
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.CategoryCode)}");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _apiClient.UpdateCategoryAsync(Code, Input);
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        TempData["SuccessMessage"] = $"Category '{Input.CategoryName}' was updated.";
        return RedirectToPage("./Index");
    }
}
