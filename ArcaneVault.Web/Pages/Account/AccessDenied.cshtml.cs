// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Account;

/// <summary>
/// Shown when a signed-in user reaches a page their role does not permit - for example a
/// collector opening a Category Management URL directly. Configured as AccessDeniedPath
/// on the cookie options in Program.cs.
/// </summary>
public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
