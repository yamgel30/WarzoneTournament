using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Discord;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IDiscordNotificationService
{
    bool IsReady { get; }
    string ConnectionStatus { get; }
    event Action? OnStatusChanged;

    Task SendMatchResultsAsync(Guid matchId, CancellationToken ct = default);
    Task SendLeaderboardUpdateAsync(Guid tournamentId, CancellationToken ct = default);
    Task NotifyTeamCheckInAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default);
    Task SendEvidenceRejectionNotificationAsync(Guid evidenceId, string reason, CancellationToken ct = default);
    Task SendTournamentAnnouncementAsync(Guid tournamentId, string message, CancellationToken ct = default);
    Task<Result<IReadOnlyList<DiscordChannelDto>>> GetGuildChannelsAsync(string guildId, CancellationToken ct = default);
    Task<Result<DiscordUserDto>> GetDiscordUserAsync(string discordId, CancellationToken ct = default);
    Task StartBotAsync(CancellationToken ct = default);
    Task StopBotAsync(CancellationToken ct = default);
}
