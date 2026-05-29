using AutoMapper;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Team;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class TeamService : ITeamService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<TeamService> _logger;
    private readonly IDiscordNotificationService _discord;

    public TeamService(IUnitOfWork uow, IMapper mapper, ILogger<TeamService> logger,
        IDiscordNotificationService discord)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _discord = discord;
    }

    public async Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, CancellationToken ct = default)
    {
        var nameExists = await _uow.Teams.ExistsAsync(t => t.Name == dto.Name, ct);
        if (nameExists)
            return Result.Failure<TeamDto>($"Team name '{dto.Name}' is already taken.");

        Player? captain = null;
        if (dto.CaptainId.HasValue && dto.CaptainId.Value != Guid.Empty)
        {
            captain = await _uow.Players.GetByIdAsync(dto.CaptainId.Value, ct);
            if (captain is null)
                return Result.Failure<TeamDto>("Captain player not found.");
            if (captain.IsBanned)
                return Result.Failure<TeamDto>("Banned players cannot be team captains.");
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var team = new Team
            {
                Name = dto.Name,
                Tag = dto.Tag,
                LogoUrl = dto.LogoUrl,
                Country = dto.Country,
                ContactEmail = dto.ContactEmail,
                PreferredPlatform = dto.PreferredPlatform,
                CaptainId = dto.CaptainId
            };

            await _uow.Teams.AddAsync(team, ct);
            await _uow.SaveChangesAsync(ct);

            // Add captain as first team member if provided
            if (captain is not null)
            {
                await _uow.TeamPlayers.AddAsync(new TeamPlayer
                {
                    TeamId = team.Id,
                    PlayerId = captain.Id,
                    Role = "Captain",
                    JoinedAt = DateTime.UtcNow
                }, ct);
            }

            // Add additional players
            foreach (var playerId in dto.PlayerIds.Where(id => id != dto.CaptainId))
            {
                var player = await _uow.Players.GetByIdAsync(playerId, ct);
                if (player is null || player.IsBanned) continue;

                await _uow.TeamPlayers.AddAsync(new TeamPlayer
                {
                    TeamId = team.Id,
                    PlayerId = playerId,
                    JoinedAt = DateTime.UtcNow
                }, ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            _logger.LogInformation("Team created: {Name} ({Id})", team.Name, team.Id);

            var result = _mapper.Map<TeamDto>(team);
            result.CaptainUsername = captain?.Username;
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Error creating team {Name}", dto.Name);
            return Result.Failure<TeamDto>("Failed to create team.");
        }
    }

    public async Task<Result<TeamDto>> GetTeamByIdAsync(Guid id, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(id, ct);
        if (team is null)
            return Result.Failure<TeamDto>("Team not found.");

        var dto = _mapper.Map<TeamDto>(team);

        var teamPlayers = await _uow.TeamPlayers.FindAsync(tp => tp.TeamId == id && tp.IsActive, ct);
        var playerDtos = new List<TeamPlayerDto>();
        foreach (var tp in teamPlayers)
        {
            var player = await _uow.Players.GetByIdAsync(tp.PlayerId, ct);
            if (player is not null)
            {
                playerDtos.Add(new TeamPlayerDto
                {
                    PlayerId = player.Id,
                    Username = player.Username,
                    ActivisionId = player.ActivisionId,
                    DiscordUsername = player.DiscordUsername,
                    Platform = player.Platform,
                    AvatarUrl = player.AvatarUrl,
                    Role = tp.Role,
                    JoinedAt = tp.JoinedAt
                });
            }
        }
        dto.Players = playerDtos;
        dto.PlayerCount = playerDtos.Count;

        if (team.CaptainId.HasValue && team.CaptainId.Value != Guid.Empty)
        {
            var captain = await _uow.Players.GetByIdAsync(team.CaptainId.Value, ct);
            dto.CaptainUsername = captain?.Username;
        }

        return Result.Success(dto);
    }

    public async Task<Result<IReadOnlyList<TeamDto>>> GetTeamsAsync(CancellationToken ct = default)
    {
        var teams = await _uow.Teams.GetAllAsync(ct);
        var dtos = _mapper.Map<IReadOnlyList<TeamDto>>(teams);
        return Result.Success(dtos);
    }

    public async Task<Result<TeamDto>> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(id, ct);
        if (team is null)
            return Result.Failure<TeamDto>("Team not found.");

        if (dto.Name is not null)
        {
            var nameExists = await _uow.Teams.ExistsAsync(t => t.Name == dto.Name && t.Id != id, ct);
            if (nameExists)
                return Result.Failure<TeamDto>($"Team name '{dto.Name}' is already taken.");
            team.Name = dto.Name;
        }
        if (dto.Tag is not null) team.Tag = dto.Tag;
        if (dto.LogoUrl is not null) team.LogoUrl = dto.LogoUrl;
        if (dto.Country is not null) team.Country = dto.Country;
        if (dto.ContactEmail is not null) team.ContactEmail = dto.ContactEmail;
        if (dto.PreferredPlatform.HasValue) team.PreferredPlatform = dto.PreferredPlatform.Value;

        _uow.Teams.Update(team);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(_mapper.Map<TeamDto>(team));
    }

    public async Task<Result> RegisterTeamForTournamentAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Team not found.");

        var tournament = await _uow.Tournaments.GetByIdAsync(tournamentId, ct);
        if (tournament is null) return Result.Failure("Tournament not found.");

        if (tournament.Status != TournamentStatus.Registration)
            return Result.Failure("Tournament is not currently accepting registrations.");

        // Validate minimum player count
        if (tournament.PlayersPerTeam > 0)
        {
            var playerCount = await _uow.TeamPlayers.CountAsync(
                tp => tp.TeamId == teamId && tp.IsActive, ct);
            if (playerCount < tournament.PlayersPerTeam)
                return Result.Failure(
                    $"El equipo necesita al menos {tournament.PlayersPerTeam} jugador(es) para inscribirse (tiene {playerCount}).");
        }

        // Check for an active registration (not soft-deleted)
        var alreadyRegistered = await _uow.TournamentTeams.ExistsAsync(
            tt => tt.TeamId == teamId && tt.TournamentId == tournamentId, ct);
        if (alreadyRegistered)
            return Result.Failure("Team is already registered for this tournament.");

        var registeredCount = await _uow.TournamentTeams.CountAsync(tt => tt.TournamentId == tournamentId, ct);
        if (registeredCount >= tournament.MaxTeams)
            return Result.Failure("Tournament is full.");

        // Purge any stale soft-deleted rows left by older code (hard delete going forward)
        var stale = await _uow.TournamentTeams.FindIncludingDeletedAsync(
            tt => tt.TeamId == teamId && tt.TournamentId == tournamentId, ct);
        foreach (var s in stale)
            _uow.TournamentTeams.HardRemove(s);

        await _uow.TournamentTeams.AddAsync(new TournamentTeam
        {
            TournamentId = tournamentId,
            TeamId = teamId
        }, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Team {TeamId} registered for tournament {TournamentId}", teamId, tournamentId);
        return Result.Success();
    }

    public async Task<Result> CheckInTeamAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default)
    {
        var tournamentTeam = await _uow.TournamentTeams.FirstOrDefaultAsync(
            tt => tt.TeamId == teamId && tt.TournamentId == tournamentId, ct);

        if (tournamentTeam is null)
            return Result.Failure("Team is not registered for this tournament.");

        var tournament = await _uow.Tournaments.GetByIdAsync(tournamentId, ct);
        if (tournament is null) return Result.Failure("Tournament not found.");

        if (tournament.Status != TournamentStatus.CheckIn)
            return Result.Failure("Tournament check-in is not currently open.");

        if (tournamentTeam.CheckedIn)
            return Result.Failure("Team is already checked in.");

        tournamentTeam.CheckedIn = true;
        tournamentTeam.CheckInTime = DateTime.UtcNow;
        _uow.TournamentTeams.Update(tournamentTeam);
        await _uow.SaveChangesAsync(ct);
        await _discord.NotifyTeamCheckInAsync(teamId, tournamentId, ct);

        return Result.Success();
    }

    public async Task<Result> AddPlayerToTeamAsync(Guid teamId, Guid playerId, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Team not found.");

        var player = await _uow.Players.GetByIdAsync(playerId, ct);
        if (player is null) return Result.Failure("Player not found.");
        if (player.IsBanned) return Result.Failure("Banned players cannot join teams.");

        var alreadyMember = await _uow.TeamPlayers.ExistsAsync(
            tp => tp.TeamId == teamId && tp.PlayerId == playerId && tp.IsActive, ct);
        if (alreadyMember) return Result.Failure("Player is already a member of this team.");

        await _uow.TeamPlayers.AddAsync(new TeamPlayer
        {
            TeamId = teamId,
            PlayerId = playerId,
            JoinedAt = DateTime.UtcNow
        }, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemovePlayerFromTeamAsync(Guid teamId, Guid playerId, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Team not found.");

        var teamPlayer = await _uow.TeamPlayers.FirstOrDefaultAsync(
            tp => tp.TeamId == teamId && tp.PlayerId == playerId && tp.IsActive, ct);
        if (teamPlayer is null) return Result.Failure("Player is not a member of this team.");

        // If removing the captain, clear the captain slot
        if (team.CaptainId == playerId)
        {
            team.CaptainId = null;
            _uow.Teams.Update(team);
        }

        teamPlayer.IsActive = false;
        teamPlayer.LeftAt = DateTime.UtcNow;
        _uow.TeamPlayers.Update(teamPlayer);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SetCaptainAsync(Guid teamId, Guid? captainPlayerId, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Team not found.");

        // Clear old captain role
        var oldCaptainEntry = await _uow.TeamPlayers.FirstOrDefaultAsync(
            tp => tp.TeamId == teamId && tp.Role == "Captain" && tp.IsActive, ct);
        if (oldCaptainEntry is not null)
        {
            oldCaptainEntry.Role = "Member";
            _uow.TeamPlayers.Update(oldCaptainEntry);
        }

        if (captainPlayerId.HasValue && captainPlayerId.Value != Guid.Empty)
        {
            var newCaptainEntry = await _uow.TeamPlayers.FirstOrDefaultAsync(
                tp => tp.TeamId == teamId && tp.PlayerId == captainPlayerId.Value && tp.IsActive, ct);
            if (newCaptainEntry is null)
                return Result.Failure("El jugador no pertenece a este equipo.");

            newCaptainEntry.Role = "Captain";
            _uow.TeamPlayers.Update(newCaptainEntry);
            team.CaptainId = captainPlayerId.Value;
        }
        else
        {
            team.CaptainId = null;
        }

        _uow.Teams.Update(team);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteTeamAsync(Guid id, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(id, ct);
        if (team is null) return Result.Failure("Team not found.");

        _uow.Teams.Remove(team);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<TournamentTeamStatusDto>>> GetTeamsWithTournamentStatusAsync(Guid tournamentId, CancellationToken ct = default)
    {
        var allTeams = await _uow.Teams.GetAllAsync(ct);
        var registeredEntries = await _uow.TournamentTeams.FindAsync(tt => tt.TournamentId == tournamentId, ct);
        var registeredMap = registeredEntries.ToDictionary(tt => tt.TeamId);

        var result = new List<TournamentTeamStatusDto>();
        foreach (var team in allTeams.OrderBy(t => t.Name))
        {
            registeredMap.TryGetValue(team.Id, out var entry);
            var teamPlayers = await _uow.TeamPlayers.FindAsync(tp => tp.TeamId == team.Id && tp.IsActive, ct);
            var playerDtos = new List<TeamPlayerSimpleDto>();
            foreach (var tp in teamPlayers)
            {
                var player = await _uow.Players.GetByIdAsync(tp.PlayerId, ct);
                if (player is not null)
                    playerDtos.Add(new TeamPlayerSimpleDto { PlayerId = player.Id, Username = player.Username });
            }
            result.Add(new TournamentTeamStatusDto
            {
                TeamId = team.Id,
                TeamName = team.Name,
                TeamTag = team.Tag,
                LogoUrl = team.LogoUrl,
                PlayerCount = teamPlayers.Count,
                IsRegistered = entry is not null,
                CheckedIn = entry?.CheckedIn ?? false,
                CheckInTime = entry?.CheckInTime,
                IsMatchPoint = entry?.IsMatchPoint ?? false,
                Players = playerDtos
            });
        }

        return Result.Success<IReadOnlyList<TournamentTeamStatusDto>>(result);
    }

    public async Task<Result> UnregisterTeamFromTournamentAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default)
    {
        var entry = await _uow.TournamentTeams.FirstOrDefaultAsync(
            tt => tt.TeamId == teamId && tt.TournamentId == tournamentId, ct);
        if (entry is null) return Result.Failure("Team is not registered for this tournament.");

        // Hard delete — soft-delete leaves a row that conflicts with the unique index on re-registration
        _uow.TournamentTeams.HardRemove(entry);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
