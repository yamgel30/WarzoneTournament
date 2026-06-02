using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages.Auth;

// Transparent relay page — never renders HTML.
// OnRemoteFailure redirects here when prompt=none fails (new user).
// This immediately fires a new Discord challenge with prompt=consent.
public class DiscordConsentModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null)
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/auth/post-login"
        };
        props.Items["prompt"] = "consent";
        return Challenge(props, "Discord");
    }
}
