using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages;

public class LoginModel : PageModel
{
    public string? ReturnUrl { get; set; }
    public string? Error { get; set; }

    public IActionResult OnGet(string? returnUrl = null, string? error = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/");

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
    {
        var redirectUri = Url.Page("/Auth/PostLogin", values: new { returnUrl });
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = redirectUri,
                IsPersistent = true,
                Items = { ["prompt"] = "none" }
            },
            "Discord");
    }
}
