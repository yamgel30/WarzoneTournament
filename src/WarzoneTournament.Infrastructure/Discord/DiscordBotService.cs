using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.DTOs.Evidence;
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
    private string GuildId => _config["Discord:GuildId"] ?? string.Empty;
    private string EvidenceChannelId => _config["Discord:EvidenceChannelId"] ?? string.Empty;
    private string AnnouncementChannelId => _config["Discord:AnnouncementChannelId"] ?? string.Empty;

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

    public async Task StartBotAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(BotToken))
        {
            _logger.LogWarning("Discord bot token not configured. Bot will not start.");
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

    public async Task SendMatchResultsAsync(Guid matchId, CancellationToken ct = default)
    {
        if (!_isReady || string.IsNullOrEmpty(AnnouncementChannelId)) return;

        try
        {
            if (ulong.TryParse(AnnouncementChannelId, out ulong channelId) &&
                _client.GetChannel(channelId) is ITextChannel channel)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Match Results Posted")
                    .WithDescription($"Match {matchId} results have been submitted for review.")
                    .WithColor(Color.Orange)
                    .WithCurrentTimestamp()
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send match results to Discord");
        }
    }

    public async Task SendLeaderboardUpdateAsync(Guid tournamentId, CancellationToken ct = default)
    {
        if (!_isReady || string.IsNullOrEmpty(AnnouncementChannelId)) return;

        try
        {
            if (ulong.TryParse(AnnouncementChannelId, out ulong channelId) &&
                _client.GetChannel(channelId) is ITextChannel channel)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Leaderboard Updated")
                    .WithDescription($"Tournament leaderboard has been updated!")
                    .WithColor(Color.Green)
                    .WithCurrentTimestamp()
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send leaderboard update to Discord");
        }
    }

    public async Task NotifyTeamCheckInAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default)
    {
        if (!_isReady || string.IsNullOrEmpty(AnnouncementChannelId)) return;

        try
        {
            if (ulong.TryParse(AnnouncementChannelId, out ulong channelId) &&
                _client.GetChannel(channelId) is ITextChannel channel)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Team Checked In")
                    .WithDescription($"A team has checked in to the tournament.")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send team check-in notification to Discord");
        }
    }

    public async Task SendEvidenceRejectionNotificationAsync(Guid evidenceId, string reason, CancellationToken ct = default)
    {
        if (!_isReady) return;

        try
        {
            if (ulong.TryParse(AnnouncementChannelId, out ulong channelId) &&
                _client.GetChannel(channelId) is ITextChannel channel)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Evidence Rejected")
                    .WithDescription($"Evidence submission was rejected.")
                    .AddField("Reason", reason)
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp()
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send evidence rejection to Discord");
        }
    }

    public async Task SendTournamentAnnouncementAsync(Guid tournamentId, string message, CancellationToken ct = default)
    {
        if (!_isReady || string.IsNullOrEmpty(AnnouncementChannelId)) return;

        try
        {
            if (ulong.TryParse(AnnouncementChannelId, out ulong channelId) &&
                _client.GetChannel(channelId) is ITextChannel channel)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Tournament Announcement")
                    .WithDescription(message)
                    .WithColor(Color.Gold)
                    .WithCurrentTimestamp()
                    .Build();

                await channel.SendMessageAsync(embed: embed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send tournament announcement to Discord");
        }
    }

    private Task OnReady()
    {
        _isReady = true;
        _logger.LogInformation("Discord bot is ready. Connected as {Username}#{Discriminator}",
            _client.CurrentUser.Username, _client.CurrentUser.Discriminator);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        // Ignore bot messages
        if (message.Author.IsBot) return;
        if (message.Channel is not ITextChannel textChannel) return;

        // Check if this is the evidence channel
        if (message.Channel.Id.ToString() != EvidenceChannelId) return;

        // Process image attachments as potential evidence
        foreach (var attachment in message.Attachments.Where(a => IsImageAttachment(a)))
        {
            _logger.LogInformation("Discord evidence image received from {Author} in channel {Channel}",
                message.Author.Username, message.Channel.Name);

            // Queue OCR processing for this image
            // In production: resolve IEvidenceService from DI and call SubmitEvidenceFromDiscordAsync
            // The match/team context would come from channel topic or bot slash commands
            await message.Channel.SendMessageAsync(
                $"Evidence received from {message.Author.Mention}. Processing...");
        }

        // Slash command handling
        if (message.Content.StartsWith("/checkin"))
        {
            await message.Channel.SendMessageAsync(
                $"{message.Author.Mention} Check-in command received. Please use the web portal to check in.");
        }
    }

    private static bool IsImageAttachment(Attachment attachment)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        return imageExtensions.Any(ext => attachment.Filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
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
