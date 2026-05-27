using Hangfire;
using Radzen;
using WarzoneTournament.Infrastructure;
using WEB.Components;
using WarzoneTournament.Infrastructure.Hubs;
using WarzoneTournament.Application;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

var app = builder.Build();

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
