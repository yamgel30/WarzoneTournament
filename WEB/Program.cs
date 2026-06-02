using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Radzen;
using WarzoneTournament.Infrastructure;
using WEB.Components;
using WarzoneTournament.Infrastructure.Hubs;
using WarzoneTournament.Application;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddOAuth("Discord", options =>
    {
        options.SignInScheme              = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ClientId                  = builder.Configuration["Discord:OAuth:ClientId"] ?? "";
        options.ClientSecret              = builder.Configuration["Discord:OAuth:ClientSecret"] ?? "";
        options.CallbackPath              = "/auth/discord-callback";
        options.AuthorizationEndpoint     = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint             = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint   = "https://discord.com/api/users/@me";
        options.Scope.Add("identify");
        options.Scope.Add("email");
        options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                using var req = new System.Net.Http.HttpRequestMessage(
                    System.Net.Http.HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", ctx.AccessToken);
                using var resp = await ctx.Backchannel.SendAsync(req);
                var doc = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync());

                var discordId  = doc.RootElement.GetProperty("id").GetString()!;
                var username   = doc.RootElement.GetProperty("username").GetString()!;
                string? email  = doc.RootElement.TryGetProperty("email",  out var ep) ? ep.GetString() : null;
                string? avatar = doc.RootElement.TryGetProperty("avatar", out var ap) ? ap.GetString() : null;

                var userSvc = ctx.HttpContext.RequestServices
                    .GetRequiredService<WarzoneTournament.Application.Common.Interfaces.IUserService>();
                var user = await userSvc.FindOrCreateByDiscordAsync(discordId, username, email, avatar);

                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()));
                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Name, user.DisplayName));
                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Role, user.Role.ToString()));
                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    "discord_id", discordId));
                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    "player_id", user.PlayerId?.ToString() ?? ""));
                ctx.Identity!.AddClaim(new System.Security.Claims.Claim(
                    "profile_complete", user.ProfileComplete ? "true" : "false"));
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});
builder.Services.AddRazorPages(); // For Login/Logout pages
builder.Services.AddCascadingAuthenticationState();

// In-memory cache (used by SiteSettingsService)
builder.Services.AddMemoryCache();

var app = builder.Build();

// Apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarzoneTournament.Infrastructure.Data.AppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var discordBot = app.Services.GetRequiredService<WarzoneTournament.Application.Common.Interfaces.IDiscordNotificationService>();
await discordBot.StartBotAsync();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();



app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Warzone Tournament Jobs",
    IsReadOnlyFunc = _ => false
});
app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.MapHub<TournamentHub>("/hubs/tournament");

app.Run();
