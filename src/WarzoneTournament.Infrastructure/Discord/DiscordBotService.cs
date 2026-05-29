using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Discord;
using WarzoneTournament.Application.DTOs.Evidence;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Infrastructure.Discord;

public class DiscordBotService : IDiscordNotificationService, IAsyncDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _isReady = false;

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

    // Resolve token: DB (SiteSettings) → appsettings fallback
    private async Task<string> GetBotTokenAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<ISiteSettingsService>();
        var dto = await settings.GetAsync();
        return !string.IsNullOrWhiteSpace(dto.DiscordBotToken)
            ? dto.DiscordBotToken
            : _config["Discord:BotToken"] ?? string.Empty;
    }

    // ── Bot lifecycle ──────────────────────────────────────────────────────

    public async Task StartBotAsync(CancellationToken ct = default)
    {
        var token = await GetBotTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Discord bot token not configured — bot will not start. Set it in Global Settings.");
            return;
        }
        await _client.LoginAsync(TokenType.Bot, token);
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
            _logger.LogWarning("Discord: error enviando resultados del match {MatchId}: {Msg}", matchId, ex.Message);
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
            _logger.LogWarning("Discord: error enviando leaderboard del torneo {TournamentId}: {Msg}", tournamentId, ex.Message);
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
            _logger.LogWarning("Discord: error enviando check-in del equipo {TeamId}: {Msg}", teamId, ex.Message);
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
            _logger.LogWarning("Discord: error enviando rechazo de evidencia {EvidenceId}: {Msg}", evidenceId, ex.Message);
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
            _logger.LogWarning("Discord: error enviando anuncio del torneo {TournamentId}: {Msg}", tournamentId, ex.Message);
        }
    }

    public Task<Result<IReadOnlyList<DiscordChannelDto>>> GetGuildChannelsAsync(string guildId, CancellationToken ct = default)
    {
        if (!_isReady)
            return Task.FromResult(Result.Failure<IReadOnlyList<DiscordChannelDto>>("El bot de Discord no está conectado."));

        if (!ulong.TryParse(guildId, out var guildIdParsed))
            return Task.FromResult(Result.Failure<IReadOnlyList<DiscordChannelDto>>("Guild ID inválido."));

        var guild = _client.GetGuild(guildIdParsed);
        if (guild is null)
            return Task.FromResult(Result.Failure<IReadOnlyList<DiscordChannelDto>>(
                "Servidor no encontrado. Verifica que el bot esté en el servidor."));

        IReadOnlyList<DiscordChannelDto> channels = guild.TextChannels
            .OrderBy(c => c.Position)
            .Select(c => new DiscordChannelDto { Id = c.Id.ToString(), Name = c.Name })
            .ToList();

        return Task.FromResult(Result.Success(channels));
    }

    public async Task<Result<DiscordUserDto>> GetDiscordUserAsync(string discordId, CancellationToken ct = default)
    {
        if (!_isReady)
            return Result.Failure<DiscordUserDto>("El bot de Discord no está configurado o no está listo.");

        if (!ulong.TryParse(discordId, out var userId))
            return Result.Failure<DiscordUserDto>("Discord ID inválido. Debe ser un número de 17–20 dígitos.");

        try
        {
            var user = await _client.Rest.GetUserAsync(userId);
            if (user is null)
                return Result.Failure<DiscordUserDto>("Usuario no encontrado en Discord.");

            return Result.Success(new DiscordUserDto
            {
                Id         = discordId,
                Username   = user.Username,
                GlobalName = user.GlobalName,
                AvatarUrl  = user.GetAvatarUrl(size: 256) ?? user.GetDefaultAvatarUrl()
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Discord: error buscando usuario {DiscordId}: {Msg}", discordId, ex.Message);
            return Result.Failure<DiscordUserDto>("No se pudo obtener la información. Verifica que el ID sea correcto.");
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

        var images = message.Attachments.Where(IsImageAttachment).ToList();
        if (!images.Any()) return;

        var channelIdStr = message.Channel.Id.ToString();
        var authorDiscordId = message.Author.Id.ToString();

        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Accept evidence from: (a) global evidence channel (DB), (b) tournament evidence channel, or (c) tournament announcement channel (fallback)
        var siteSettingsSvc = scope.ServiceProvider.GetRequiredService<ISiteSettingsService>();
        var siteSettings = await siteSettingsSvc.GetAsync();
        var globalEvidenceChannelId = !string.IsNullOrWhiteSpace(siteSettings.DefaultDiscordEvidenceChannelId)
            ? siteSettings.DefaultDiscordEvidenceChannelId
            : _config["Discord:EvidenceChannelId"];
        bool isEvidenceChannel = channelIdStr == globalEvidenceChannelId;

        if (!isEvidenceChannel)
        {
            var tournament = await uow.Tournaments.FirstOrDefaultAsync(
                t => t.Status == TournamentStatus.InProgress &&
                     (t.DiscordEvidenceChannelId == channelIdStr ||
                      (t.DiscordEvidenceChannelId == null && t.DiscordChannelId == channelIdStr)));
            isEvidenceChannel = tournament is not null;
        }

        if (!isEvidenceChannel) return;

        _logger.LogInformation("Discord: {Count} imagen(es) de evidencia de {Author} en #{Channel}",
            images.Count, message.Author.Username, message.Channel.Name);

        // Identify player by Discord ID
        var playerService = scope.ServiceProvider.GetRequiredService<IPlayerService>();
        var playerResult = await playerService.GetPlayerByDiscordIdAsync(authorDiscordId);

        if (playerResult.IsFailure)
        {
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} ❌ No encontré tu jugador registrado (Discord ID: `{authorDiscordId}`). " +
                $"Pide al administrador que te registre en el sistema.");
            return;
        }

        // Resolve active tournament + match for this player
        var contextResult = await playerService.GetPlayerTournamentContextAsync(playerResult.Value.Id);
        if (contextResult.IsFailure)
        {
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} ⚠️ {contextResult.Error}");
            return;
        }

        var ctx = contextResult.Value;
        if (ctx.ActiveMatchId is null)
        {
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} ⚠️ No hay mapa activo para el equipo **{ctx.TeamName}** en este momento.");
            return;
        }

        // Submit each image as a separate evidence record
        var evidenceService = scope.ServiceProvider.GetRequiredService<IEvidenceService>();
        int submitted = 0;

        foreach (var img in images)
        {
            // Use "{messageId}_{attachmentId}" so multiple images in one message each get a unique key
            var attachmentKey = $"{message.Id}_{img.Id}";

            var dto = new DiscordEvidenceDto
            {
                MatchId            = ctx.ActiveMatchId.Value,
                SubmittedByTeamId  = ctx.TeamId,
                SubmittedByPlayerId = ctx.PlayerId,
                ImageUrl           = img.Url,
                DiscordMessageId   = attachmentKey,
                DiscordChannelId   = channelIdStr,
                DiscordUsername    = message.Author.Username,
                OriginalFileName   = img.Filename,
                FileSizeBytes      = img.Size
            };

            var result = await evidenceService.SubmitEvidenceFromDiscordAsync(dto);
            if (result.IsSuccess)
            {
                submitted++;
            }
            else
            {
                _logger.LogWarning("Discord evidence submit failed for {Author}: {Error}",
                    message.Author.Username, result.Error);
            }
        }

        if (submitted > 0)
        {
            var word = submitted == 1 ? "imagen registrada" : "imágenes registradas";
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} ✅ **{submitted} {word}** para el " +
                $"**Mapa #{ctx.ActiveMatchNumber}** — equipo **{ctx.TeamName}**.\n" +
                $"El administrador revisará la evidencia y confirmará los resultados.");
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
