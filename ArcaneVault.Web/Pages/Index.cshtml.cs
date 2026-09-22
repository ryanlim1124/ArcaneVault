// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages;

/// <summary>
/// Public landing page. Deliberately anonymous-accessible so visitors can see what the
/// platform does before registering; the content it shows adapts to whether the visitor
/// is signed in and which role they hold.
/// </summary>
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
