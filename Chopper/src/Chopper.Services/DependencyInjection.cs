using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Chopper.Services.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace Chopper.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddChopperServices(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddScoped<IPatientService, PatientService>();

        return services;
    }
}
