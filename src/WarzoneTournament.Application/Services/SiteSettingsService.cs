using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Settings;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<SiteSettingsService> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "site_settings";

    public SiteSettingsService(IUnitOfWork uow, ILogger<SiteSettingsService> logger, IMemoryCache cache)
    {
        _uow = uow;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SiteSettingsDto> GetAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteSettingsDto? cached))
            return cached!;

        var settings = (await _uow.SiteSettings.GetAllAsync(ct)).FirstOrDefault();
        var dto = settings is null ? new SiteSettingsDto() : new SiteSettingsDto
        {
            AppName                             = settings.AppName,
            SupportEmail                        = settings.SupportEmail,
            DefaultLogoUrl                      = settings.DefaultLogoUrl,
            DefaultBannerUrl                    = settings.DefaultBannerUrl,
            DefaultPlacementPointsJson          = settings.DefaultPlacementPointsJson,
            DefaultMatchPointThreshold          = settings.DefaultMatchPointThreshold,
            DiscordBotToken                     = settings.DiscordBotToken,
            DefaultDiscordGuildId               = settings.DefaultDiscordGuildId,
            DefaultDiscordAnnouncementChannelId = settings.DefaultDiscordAnnouncementChannelId,
            DefaultDiscordEvidenceChannelId     = settings.DefaultDiscordEvidenceChannelId,
            FeaturedTournamentId                = settings.FeaturedTournamentId
        };

        _cache.Set(CacheKey, dto, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(5)
        });
        return dto;
    }

    public async Task<Result<SiteSettingsDto>> SaveAsync(SiteSettingsDto dto, CancellationToken ct = default)
    {
        try
        {
            var all = await _uow.SiteSettings.GetAllAsync(ct);
            var settings = all.FirstOrDefault();

            if (settings is null)
            {
                settings = new SiteSettings();
                MapDto(dto, settings);
                await _uow.SiteSettings.AddAsync(settings, ct);
            }
            else
            {
                MapDto(dto, settings);
                _uow.SiteSettings.Update(settings);
            }

            await _uow.SaveChangesAsync(ct);
            _cache.Remove(CacheKey);
            _logger.LogInformation("Site settings saved.");
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving site settings.");
            return Result.Failure<SiteSettingsDto>("Error al guardar la configuración.");
        }
    }

    private static void MapDto(SiteSettingsDto dto, SiteSettings entity)
    {
        entity.AppName                           = dto.AppName;
        entity.SupportEmail                      = dto.SupportEmail;
        entity.DefaultLogoUrl                    = dto.DefaultLogoUrl;
        entity.DefaultBannerUrl                  = dto.DefaultBannerUrl;
        entity.DefaultPlacementPointsJson        = dto.DefaultPlacementPointsJson;
        entity.DefaultMatchPointThreshold        = dto.DefaultMatchPointThreshold;
        entity.DiscordBotToken                   = dto.DiscordBotToken;
        entity.DefaultDiscordGuildId             = dto.DefaultDiscordGuildId;
        entity.DefaultDiscordAnnouncementChannelId = dto.DefaultDiscordAnnouncementChannelId;
        entity.DefaultDiscordEvidenceChannelId   = dto.DefaultDiscordEvidenceChannelId;
        entity.FeaturedTournamentId              = dto.FeaturedTournamentId;
    }
}
