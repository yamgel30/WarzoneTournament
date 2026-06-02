using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages.Auth;

public class DiscordConsentModel : PageModel
{
    public IActionResult OnGet(string? returnUrl = null)
    {
        // Signal OnRedirectToAuthorizationEndpoint (same request) to swap prompt=none → consent
        HttpContext.Items["discord_prompt"] = "consent";

        var props = new AuthenticationProperties { RedirectUri = returnUrl ?? "/auth/post-login" };
        // Also encode in state so OnRemoteFailure can detect user-denied vs silent-fail
        props.Items["discord_prompt"] = "consent";
        return Challenge(props, "Discord");
    }
}
