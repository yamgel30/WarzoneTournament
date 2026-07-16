using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WEB.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IConfiguration _config;

    public LoginModel(IConfiguration config) => _config = config;

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(returnUrl ?? "/admin");
        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var adminUser = _config["AdminCredentials:Username"] ?? "admin";
        var adminPass = _config["AdminCredentials:Password"] ?? "admin123";

        if (!string.Equals(Username.Trim(), adminUser, StringComparison.Ordinal) ||
            !string.Equals(Password, adminPass, StringComparison.Ordinal))
        {
            ErrorMessage = "Usuario o contraseña incorrectos.";
            ReturnUrl = returnUrl;
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, adminUser),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        var destination = Url.IsLocalUrl(returnUrl) ? returnUrl : "/admin";
        return Redirect(destination);
    }
}
