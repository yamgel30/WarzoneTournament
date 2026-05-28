using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Settings;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface ISiteSettingsService
{
    Task<SiteSettingsDto> GetAsync(CancellationToken ct = default);
    Task<Result<SiteSettingsDto>> SaveAsync(SiteSettingsDto dto, CancellationToken ct = default);
}
