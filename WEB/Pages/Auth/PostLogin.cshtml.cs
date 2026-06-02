using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages.Auth;

[Authorize]
public class PostLoginModel : PageModel
{
    public string RedirectTo { get; private set; } = "/";

    public IActionResult OnGet(string? returnUrl = null)
    {
        var profileComplete = User.FindFirst("profile_complete")?.Value == "true";
        if (!profileComplete)
        {
            RedirectTo = "/profile/setup";
            return Page();
        }
        var destination = Url.IsLocalUrl(returnUrl) ? returnUrl! : "/";
        return Redirect(destination);
    }
}
