// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using System.Security.Claims;
using ArcaneVault.Web.Models;
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;

/// <summary>
/// Sign-in. Credential checking belongs to the API (it is the only tier that can see the
/// stored password hash); this page's job is to turn a successful check into a session.
/// </summary>
public class LoginModel : PageModel
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(ApiClient apiClient, ILogger<LoginModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [BindProperty]
    public LoginInputDto Input { get; set; } = new();

    /// <summary>
    /// Where to send the user afterwards. Populated automatically when the cookie
    /// middleware bounces an unauthenticated request here, so they land back on the page
    /// they originally wanted.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
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

        UserDto user;
        try
        {
            // The API verifies the password against the stored PBKDF2 hash and returns
            // the account's details - including which role it holds.
            user = await _apiClient.LoginAsync(Input);
        }
        catch (ApiClientException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        // Build the identity that will be carried in the authentication cookie.
        // The Role claim is what powers both User.IsInRole("Staff") in the navbar and the
        // "StaffOnly" authorisation policy - it originates from the database, never the browser.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        _logger.LogInformation("{UserName} signed in as {Role}", user.UserName, user.RoleName);

        TempData["SuccessMessage"] = $"Signed in as {user.UserName}.";

        // Only follow a local return URL. Honouring an absolute URL here would be an
        // open-redirect vulnerability, letting a crafted link bounce users off-site.
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        // Staff land on the analytics dashboard; collectors land on their collection.
        return user.RoleName == "Staff"
            ? RedirectToPage("/Insights/Index")
            : RedirectToPage("/CollectionItems/Index");
    }
}
