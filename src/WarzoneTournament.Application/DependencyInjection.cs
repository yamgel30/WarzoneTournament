using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Services;

namespace WarzoneTournament.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ITournamentService, TournamentService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IEvidenceService, EvidenceService>();
        services.AddScoped<IRoundService, RoundService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();

        return services;
    }
}
