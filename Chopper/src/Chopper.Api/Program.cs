using Chopper.Services;
using Chopper.Services.ClaimSearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("ChopperDb")
    ?? throw new InvalidOperationException("Missing 'ConnectionStrings:ChopperDb' configuration value.");

var ahaSearchOptions = new AhaSearchOptions
{
    DefaultYear = builder.Configuration.GetValue("AhaSearch:DefaultYear", DateTime.UtcNow.Year),
    DefaultClaimClass = builder.Configuration.GetValue("AhaSearch:DefaultClaimClass", 0),
    MaxDayToResubmit = builder.Configuration.GetValue("AhaSearch:MaxDayToResubmit", 0),
};
builder.Services.AddChopperServices(connectionString, ahaSearchOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
