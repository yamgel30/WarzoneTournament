using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Infrastructure.Discord;

public class DiscordBotService : IDiscordNotificationService, IAsyncDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _isReady = false;

    private string BotToken => _config["Discord:BotToken"] ?? string.Empty;

    public DiscordBotService(IConfiguration config, ILogger<DiscordBotService> logger,
        IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                | GatewayIntents.GuildMessages
                | GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Info
        });

        _client.Log += OnLog;
        _client.Ready += OnReady;
        _client.MessageReceived += OnMessageReceived;
    }

    // ── Bot lifecycle ──────────────────────────────────────────────────────

    public async Task StartBotAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(BotToken))
        {
            _logger.LogWarning("Discord:BotToken not configured — bot will not start.");
            return;
        }
        await _client.LoginAsync(TokenType.Bot, BotToken);
        await _client.StartAsync();
        _logger.LogInformation("Discord bot started.");
    }

    public async Task StopBotAsync(CancellationToken ct = default)
    {
        await _client.StopAsync();
        _logger.LogInformation("Discord bot stopped.");
    }

    // ── Notifications ──────────────────────────────────────────────────────

    public async Task SendMatchResultsAsync(Guid matchId, CancellationToken ct = default)
    {
        if (!_isReady) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var match = await uow.Matches.GetByIdAsync(matchId, ct);
            if (match is null) return;

            var channel = await GetTournamentChannelAsync(match.TournamentId, uow);
            if (channel is null) return;

            var tournament = await uow.Tournaments.GetByIdAsync(match.TournamentId, ct);
            var results = await uow.MatchTeamResults.FindAsync(r => r.MatchId == matchId, ct);
            var sorted = results.OrderBy(r => r.Placement).ToList();

            var embed = new EmbedBuilder()
                .WithTitle($"🗺️ Mapa #{match.MatchNumber} — Resultados")
                .WithDescription(tournament?.Name ?? "Torneo")
                .WithColor(Color.Orange)
                .WithCurrentTimestamp();

            foreach (var r in sorted.Take(10))
            {
                var team = await uow.Teams.GetByIdAsync(r.TeamId, ct);
                var medal = r.Placement switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{r.Placement}" };
                embed.AddField(
                    $"{medal} {team?.Name ?? "Equipo"}",
                    $"Kills: **{r.Kills}** · Puntos: **{r.TotalPoints}**",
                    inline: true);
            }

            await channel.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord: error sending match results for {MatchId}", matchId);
        }
    }

    public async Task SendLeaderboardUpdateAsync(Guid tournamentId, CancellationToken ct = default)
    {
        if (!_isReady) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var leaderboard = scope.ServiceProvider.GetRequiredService<ILeaderboardService>();

            var channel = await GetTournamentChannelAsync(tournamentId, uow);
            if (channel is null) return;

            var tournament = await uow.Tournaments.GetByIdAsync(tournamentId, ct);
            var lb = await leaderboard.GetTournamentLeaderboardAsync(tournamentId, ct);
            if (!lb.IsSuccess || !lb.Value.Any()) return;

            var embed = new EmbedBuilder()
                .WithTitle($"🏆 Leaderboard actualizado — {tournament?.Name}")
                .WithColor(Color.Gold)
                .WithCurrentTimestamp();

            foreach (var entry in lb.Value.Take(5))
            {
                var medal = entry.Rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{entry.Rank}" };
                embed.AddField(
                    $"{medal} {entry.TeamName}",
                    $"Pts: **{entry.TotalPoints}** · Kills: {entry.TotalKills} · Mapas: {entry.MatchesPlayed}",
                    inline: false);
            }

            await channel.SendMessageAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord: error sending leaderboard for tournament {TournamentId}", tournamentId);
        }
    }

    public async Task NotifyTeamCheckInAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default)
    {
        if (!_isReady) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var channel = await GetTournamentChannelAsync(tournamentId, uow);
            if (channel is null) return;

            var team = await uow.Teams.GetByIdAsync(teamId, ct);
            var tournament = await uow.Tournaments.GetByIdAsync(tournamentId, ct);

            var embed = new EmbedBuilder()
                .WithTitle("✅ Check-In confirmado")
                .WithDescription($"**{team?.Name ?? "Equipo"}** confirmó asistencia en **{tournament?.Name}**.")
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord: error sending check-in for team {TeamId}", teamId);
        }
    }

    public async Task SendEvidenceRejectionNotificationAsync(Guid evidenceId, string reason, CancellationToken ct = default)
    {
        if (!_isReady) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var evidence = await uow.MatchEvidences.GetByIdAsync(evidenceId, ct);
            if (evidence is null) return;

            var match = await uow.Matches.GetByIdAsync(evidence.MatchId, ct);
            if (match is null) return;

            var channel = await GetTournamentChannelAsync(match.TournamentId, uow);
            if (channel is null) return;

            var embed = new EmbedBuilder()
                .WithTitle("❌ Evidencia rechazada")
                .WithDescription($"Una evidencia del Mapa #{match.MatchNumber} fue rechazada.")
                .AddField("Razón", reason)
                .WithColor(Color.Red)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord: error sending evidence rejection for {EvidenceId}", evidenceId);
        }
    }

    public async Task SendTournamentAnnouncementAsync(Guid tournamentId, string message, CancellationToken ct = default)
    {
        if (!_isReady) return;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var channel = await GetTournamentChannelAsync(tournamentId, uow);
            if (channel is null) return;

            var tournament = await uow.Tournaments.GetByIdAsync(tournamentId, ct);

            var embed = new EmbedBuilder()
                .WithTitle($"📢 {tournament?.Name}")
                .WithDescription(message)
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discord: error sending announcement for tournament {TournamentId}", tournamentId);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<ITextChannel?> GetTournamentChannelAsync(Guid tournamentId, IUnitOfWork uow)
    {
        var tournament = await uow.Tournaments.GetByIdAsync(tournamentId);
        if (tournament is null || string.IsNullOrEmpty(tournament.DiscordChannelId))
        {
            _logger.LogDebug("Tournament {Id} has no Discord channel configured — skipping notification.", tournamentId);
            return null;
        }

        if (!ulong.TryParse(tournament.DiscordChannelId, out var channelId))
        {
            _logger.LogWarning("Tournament {Id} has invalid DiscordChannelId '{Val}'.", tournamentId, tournament.DiscordChannelId);
            return null;
        }

        return _client.GetChannel(channelId) as ITextChannel;
    }

    // ── Event handlers ─────────────────────────────────────────────────────

    private Task OnReady()
    {
        _isReady = true;
        _logger.LogInformation("Discord bot ready. Connected as {User}.", _client.CurrentUser.Username);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        if (message.Channel is not ITextChannel) return;

        var channelIdStr = message.Channel.Id.ToString();

        // Check if this channel belongs to any active tournament
        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var tournaments = await uow.Tournaments.FindAsync(t => t.DiscordChannelId == channelIdStr);
        if (!tournaments.Any()) return;

        // Process image attachments as evidence
        var images = message.Attachments.Where(IsImageAttachment).ToList();
        if (images.Any())
        {
            _logger.LogInformation("Discord evidence image from {Author} in channel {Channel}",
                message.Author.Username, message.Channel.Name);
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} Screenshot recibido ✅. El admin lo revisará.");
        }
    }

    private static bool IsImageAttachment(Attachment a)
    {
        var exts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return exts.Any(e => a.Filename.EndsWith(e, StringComparison.OrdinalIgnoreCase));
    }

    private Task OnLog(LogMessage log)
    {
        _logger.LogInformation("[Discord] {Severity} {Source}: {Message}", log.Severity, log.Source, log.Message);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _client.Log -= OnLog;
        _client.Ready -= OnReady;
        _client.MessageReceived -= OnMessageReceived;
        await _client.DisposeAsync();
    }
}
