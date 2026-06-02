using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages;

public class LoginModel : PageModel
{
    public string? ReturnUrl { get; set; }
    public string? Error { get; set; }

    public IActionResult OnGet(string? returnUrl = null, string? error = null, bool consent = false)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/");

        // Silent auth (prompt=none) failed for a new user — auto-retry with consent dialog
        if (consent)
            return DiscordChallenge(returnUrl, "consent");

        ReturnUrl = returnUrl;
        Error = error switch
        {
            "access_denied" => "Denegaste el acceso a Discord.",
            "discord_unavailable" => "Discord no está disponible. Intenta de nuevo.",
            _ => null
        };
        return Page();
    }

    public IActionResult OnPostDiscord(string? returnUrl = null)
        => DiscordChallenge(returnUrl, "none");

    private IActionResult DiscordChallenge(string? returnUrl, string prompt)
    {
        var redirectUri = Url.Page("/Auth/PostLogin", values: new { returnUrl });
        var props = new AuthenticationProperties { RedirectUri = redirectUri };
        props.Items["prompt"] = prompt;
        return Challenge(props, "Discord");
    }
}
