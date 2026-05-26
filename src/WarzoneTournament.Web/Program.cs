using Hangfire;
using Radzen;
using WarzoneTournament.Application;
using WarzoneTournament.Infrastructure;
using WarzoneTournament.Infrastructure.Data;

using WarzoneTournament.Web.Components;
using WarzoneTournament.Web.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddRadzenComponents();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();

// Auto-migrate on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database migration failed. Ensure SQL Server is running.");
    }
}



// Start Discord bot
var discordBot = app.Services.GetRequiredService<WarzoneTournament.Application.Common.Interfaces.IDiscordNotificationService>();
await discordBot.StartBotAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();


app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Warzone Tournament Jobs",
    IsReadOnlyFunc = _ => false
});

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapHub<TournamentHub>("/hubs/tournament");

app.Run();
