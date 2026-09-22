// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;

/// <summary>
/// Ends the session by clearing the authentication cookie.
/// Handled as a POST (the navbar renders a small form, not a link) so that logging out
/// can only ever be a deliberate action, never something a stray link could trigger.
/// </summary>
public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    /// <summary>A direct GET to /Account/Logout just returns home - nothing to do.</summary>
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        var userName = User.Identity?.Name;

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("{UserName} signed out", userName ?? "(unknown)");

        TempData["SuccessMessage"] = "You have been signed out.";
        return RedirectToPage("/Index");
    }
}
