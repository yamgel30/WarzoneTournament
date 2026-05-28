using AutoMapper;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Player;
using WarzoneTournament.Application.DTOs.Team;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class PlayerService : IPlayerService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(IUnitOfWork uow, IMapper mapper, ILogger<PlayerService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PlayerDto>> CreatePlayerAsync(CreatePlayerDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(dto.DiscordId))
        {
            var existsByDiscord = await _uow.Players.ExistsAsync(p => p.DiscordId == dto.DiscordId, ct);
            if (existsByDiscord)
                return Result.Failure<PlayerDto>("A player with this Discord ID already exists.");
        }

        var existsByUsername = await _uow.Players.ExistsAsync(p => p.Username == dto.Username, ct);
        if (existsByUsername)
            return Result.Failure<PlayerDto>($"Username '{dto.Username}' is already taken.");

        var player = _mapper.Map<Player>(dto);
        await _uow.Players.AddAsync(player, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Player created: {Username} ({Id})", player.Username, player.Id);
        return Result.Success(_mapper.Map<PlayerDto>(player));
    }

    public async Task<Result<PlayerDto>> GetPlayerByIdAsync(Guid id, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(id, ct);
        if (player is null)
            return Result.Failure<PlayerDto>("Player not found.");

        return Result.Success(_mapper.Map<PlayerDto>(player));
    }

    public async Task<Result<PlayerDto>> GetPlayerByDiscordIdAsync(string discordId, CancellationToken ct = default)
    {
        var player = await _uow.Players.FirstOrDefaultAsync(p => p.DiscordId == discordId, ct);
        if (player is null)
            return Result.Failure<PlayerDto>("Player not found.");

        return Result.Success(_mapper.Map<PlayerDto>(player));
    }

    public async Task<Result<IReadOnlyList<PlayerDto>>> GetPlayersAsync(CancellationToken ct = default)
    {
        var players = await _uow.Players.GetAllAsync(ct);
        return Result.Success(_mapper.Map<IReadOnlyList<PlayerDto>>(players));
    }

    public async Task<Result<PlayerDto>> UpdatePlayerAsync(Guid id, UpdatePlayerDto dto, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(id, ct);
        if (player is null)
            return Result.Failure<PlayerDto>("Player not found.");

        if (dto.Username is not null)
        {
            var usernameExists = await _uow.Players.ExistsAsync(p => p.Username == dto.Username && p.Id != id, ct);
            if (usernameExists)
                return Result.Failure<PlayerDto>($"Username '{dto.Username}' is already taken.");
            player.Username = dto.Username;
        }
        if (dto.ActivisionId is not null) player.ActivisionId = dto.ActivisionId;
        if (dto.DiscordId is not null) player.DiscordId = dto.DiscordId;
        if (dto.DiscordUsername is not null) player.DiscordUsername = dto.DiscordUsername;
        if (dto.Email is not null) player.Email = dto.Email;
        if (dto.Platform.HasValue) player.Platform = dto.Platform.Value;
        if (dto.Country is not null) player.Country = dto.Country;
        if (dto.AvatarUrl is not null) player.AvatarUrl = dto.AvatarUrl;

        _uow.Players.Update(player);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(_mapper.Map<PlayerDto>(player));
    }

    public async Task<Result<PlayerStatsDto>> GetPlayerStatsAsync(Guid playerId, Guid? tournamentId = null, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null)
            return Result.Failure<PlayerStatsDto>("Player not found.");

        var stats = await _uow.PlayerMatchStats.FindAsync(ps => ps.PlayerId == playerId, ct);

        if (tournamentId.HasValue)
        {
            var matchIds = _uow.Matches.Query()
                .Where(m => m.TournamentId == tournamentId.Value)
                .Select(m => m.Id)
                .ToHashSet();
            stats = stats.Where(s => matchIds.Contains(s.MatchId)).ToList();
        }

        var matchResults = new List<RecentMatchStatsDto>();
        foreach (var stat in stats.OrderByDescending(s => s.CreatedAt).Take(10))
        {
            var match = await _uow.Matches.GetByIdAsync(stat.MatchId, ct);
            var teamResult = await _uow.MatchTeamResults.FirstOrDefaultAsync(
                r => r.MatchId == stat.MatchId && r.TeamId == stat.TeamId, ct);
            var tournament = match is not null ? await _uow.Tournaments.GetByIdAsync(match.TournamentId, ct) : null;

            matchResults.Add(new RecentMatchStatsDto
            {
                MatchId = stat.MatchId,
                TournamentName = tournament?.Name,
                MatchDate = match?.StartTime,
                Kills = stat.Kills,
                Deaths = stat.Deaths,
                Damage = stat.Damage,
                TeamPlacement = teamResult?.Placement ?? 0
            });
        }

        var dto = new PlayerStatsDto
        {
            PlayerId = playerId,
            Username = player.Username,
            TotalKills = stats.Sum(s => s.Kills),
            TotalDeaths = stats.Sum(s => s.Deaths),
            TotalAssists = stats.Sum(s => s.Assists),
            TotalDamage = stats.Sum(s => s.Damage),
            TotalMatches = stats.Select(s => s.MatchId).Distinct().Count(),
            TotalRevives = stats.Sum(s => s.Revives),
            BestKillGame = stats.Any() ? stats.Max(s => s.Kills) : 0,
            RecentMatches = matchResults
        };

        return Result.Success(dto);
    }

    public async Task<Result> BanPlayerAsync(Guid playerId, string reason, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null) return Result.Failure("Player not found.");

        player.IsBanned = true;
        player.BanReason = reason;
        _uow.Players.Update(player);
        await _uow.SaveChangesAsync(ct);

        _logger.LogWarning("Player {Username} ({Id}) banned: {Reason}", player.Username, playerId, reason);
        return Result.Success();
    }

    public async Task<Result> UnbanPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null) return Result.Failure("Player not found.");

        player.IsBanned = false;
        player.BanReason = null;
        _uow.Players.Update(player);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<PlayerContextDto>> GetPlayerTournamentContextAsync(Guid playerId, CancellationToken ct = default)
    {
        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null) return Result.Failure<PlayerContextDto>("Jugador no encontrado.");

        var teamPlayers = await _uow.TeamPlayers.FindAsNoTrackingAsync(
            tp => tp.PlayerId == playerId && tp.IsActive, ct);

        if (!teamPlayers.Any())
            return Result.Failure<PlayerContextDto>("No estás en ningún equipo activo.");

        foreach (var tp in teamPlayers)
        {
            var tournamentTeams = await _uow.TournamentTeams.FindAsNoTrackingAsync(
                x => x.TeamId == tp.TeamId, ct);

            foreach (var tt in tournamentTeams)
            {
                var tournament = await _uow.Tournaments.GetByIdAsync(tt.TournamentId, ct);
                if (tournament?.Status != TournamentStatus.InProgress) continue;

                var team = await _uow.Teams.GetByIdAsync(tp.TeamId, ct);

                var matches = await _uow.Matches.FindAsNoTrackingAsync(
                    m => m.TournamentId == tt.TournamentId &&
                         (m.Status == Domain.Enums.MatchStatus.InProgress || m.Status == Domain.Enums.MatchStatus.Pending), ct);
                // InProgress takes strict priority over Pending so evidence always targets the running map
                var activeMatch = matches
                    .OrderByDescending(m => m.Status == Domain.Enums.MatchStatus.InProgress ? 1 : 0)
                    .ThenByDescending(m => m.MatchNumber)
                    .FirstOrDefault();

                var allTeamPlayers = await _uow.TeamPlayers.FindAsNoTrackingAsync(
                    x => x.TeamId == tp.TeamId && x.IsActive, ct);
                var playerSimples = new List<TeamPlayerSimpleDto>();
                foreach (var p in allTeamPlayers)
                {
                    var pl = await _uow.Players.GetByIdAsync(p.PlayerId, ct);
                    if (pl is not null)
                        playerSimples.Add(new TeamPlayerSimpleDto { PlayerId = p.PlayerId, Username = pl.Username });
                }

                return Result.Success(new PlayerContextDto
                {
                    PlayerId = playerId,
                    PlayerName = player.Username,
                    TeamId = tp.TeamId,
                    TeamName = team?.Name ?? "Desconocido",
                    TournamentId = tt.TournamentId,
                    TournamentName = tournament.Name,
                    ActiveMatchId = activeMatch?.Id,
                    ActiveMatchNumber = activeMatch?.MatchNumber ?? 0,
                    Players = playerSimples
                });
            }
        }

        return Result.Failure<PlayerContextDto>("Tu equipo no está participando en ningún torneo activo en este momento.");
    }
}
