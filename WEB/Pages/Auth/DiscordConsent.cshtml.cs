using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WEB.Pages.Auth;

public class DiscordConsentModel : PageModel
{
    public IActionResult OnGet()
    {
        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = "/auth/post-login",
                IsPersistent = true,
                Items = { ["prompt"] = "consent" }
            },
            "Discord");
    }
}
