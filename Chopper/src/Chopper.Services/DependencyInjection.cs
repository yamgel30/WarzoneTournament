using Chopper.Services.Abstractions;
using Chopper.Services.Acknowledgements;
using Chopper.Services.AhaClaims;
using Chopper.Services.AiInfo;
using Chopper.Services.Claims;
using Chopper.Services.ClaimSearch;
using Chopper.Services.Common;
using Chopper.Services.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Chopper.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddChopperServices(this IServiceCollection services, string connectionString, AhaSearchOptions ahaSearchOptions)
    {
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddSingleton(ahaSearchOptions);
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAcknowledgementService, AcknowledgementService>();
        services.AddScoped<IClaimsVerificationService, ClaimsVerificationService>();
        services.AddScoped<IAiInfoService, AiInfoService>();
        services.AddScoped<IAhaClaimService, AhaClaimService>();
        services.AddScoped<IClaimSearchService, ClaimSearchService>();

        return services;
    }
}
