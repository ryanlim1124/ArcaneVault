// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;

/// <summary>
/// Account registration. Validation happens in three layers:
///  1. In the browser, from the DataAnnotations on <see cref="RegisterInputDto"/>.
///  2. Here on the server, via ModelState - which catches anyone bypassing the browser.
///  3. In the API, which owns the rules this page cannot see (duplicate username/email).
/// </summary>
public class RegisterModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(ApiClient apiClient, ILogger<RegisterModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [BindProperty]
    public RegisterInputDto Input { get; set; } = new();

    public IActionResult OnGet()
    {
        // Someone already signed in has no reason to be here.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var user = await _apiClient.RegisterAsync(Input);
            _logger.LogInformation("Account created for {UserName}", user.UserName);
        }
        catch (ApiClientException ex)
        {
            // Surface the API's own message (e.g. "An account already exists for ...")
            // as a form-level error rather than crashing the page.
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        TempData["SuccessMessage"] = $"Welcome to Arcane Vault, {Input.UserName}! Your account has been created - please sign in.";
        return RedirectToPage("/Account/Login");
    }
}
