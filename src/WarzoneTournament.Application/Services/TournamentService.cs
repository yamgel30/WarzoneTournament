using AutoMapper;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Tournament;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class TournamentService : ITournamentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<TournamentService> _logger;
    private readonly ISignalRNotificationService _signalR;
    private readonly IDiscordNotificationService _discord;

    public TournamentService(IUnitOfWork uow, IMapper mapper,
        ILogger<TournamentService> logger, ISignalRNotificationService signalR,
        IDiscordNotificationService discord)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _signalR = signalR;
        _discord = discord;
    }

    public async Task<Result<TournamentDto>> CreateTournamentAsync(CreateTournamentDto dto, CancellationToken ct = default)
    {
        try
        {
            var tournament = _mapper.Map<Tournament>(dto);
            await _uow.Tournaments.AddAsync(tournament, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Tournament created: {Name} ({Id})", tournament.Name, tournament.Id);
            return Result.Success(_mapper.Map<TournamentDto>(tournament));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tournament");
            return Result.Failure<TournamentDto>("Failed to create tournament.");
        }
    }

    public async Task<Result<TournamentDto>> UpdateTournamentAsync(Guid id, UpdateTournamentDto dto, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status == TournamentStatus.Completed || tournament.Status == TournamentStatus.Cancelled)
            return Result.Failure<TournamentDto>("Cannot update a completed or cancelled tournament.");

        if (dto.Name is not null) tournament.Name = dto.Name;
        if (dto.Description is not null) tournament.Description = dto.Description;
        if (dto.Platform.HasValue) tournament.Platform = dto.Platform.Value;
        if (dto.MaxTeams.HasValue) tournament.MaxTeams = dto.MaxTeams.Value;
        if (dto.PlayersPerTeam.HasValue) tournament.PlayersPerTeam = dto.PlayersPerTeam.Value;
        if (dto.RegistrationStart.HasValue) tournament.RegistrationStart = dto.RegistrationStart.Value;
        if (dto.RegistrationEnd.HasValue) tournament.RegistrationEnd = dto.RegistrationEnd.Value;
        if (dto.CheckInStart.HasValue) tournament.CheckInStart = dto.CheckInStart.Value;
        if (dto.CheckInEnd.HasValue) tournament.CheckInEnd = dto.CheckInEnd.Value;
        if (dto.StartDate.HasValue) tournament.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue) tournament.EndDate = dto.EndDate.Value;
        if (dto.PrizePool.HasValue) tournament.PrizePool = dto.PrizePool.Value;
        if (dto.TournamentRulesText is not null) tournament.TournamentRulesText = dto.TournamentRulesText;
        if (dto.StreamUrl is not null) tournament.StreamUrl = dto.StreamUrl;
        if (dto.DiscordChannelId is not null) tournament.DiscordChannelId = dto.DiscordChannelId;
        if (dto.DiscordGuildId is not null) tournament.DiscordGuildId = dto.DiscordGuildId;
        if (dto.KillPoints.HasValue) tournament.KillPoints = dto.KillPoints.Value;
        if (dto.PlacementPointsJson is not null) tournament.PlacementPointsJson = dto.PlacementPointsJson;
        if (dto.IsPrivate.HasValue) tournament.IsPrivate = dto.IsPrivate.Value;
        if (dto.LobbyCode is not null) tournament.LobbyCode = dto.LobbyCode;
        if (dto.LobbyPassword is not null) tournament.LobbyPassword = dto.LobbyPassword;
        if (dto.BannerImageUrl is not null) tournament.BannerImageUrl = dto.BannerImageUrl;
        if (dto.LogoUrl is not null) tournament.LogoUrl = dto.LogoUrl;
        if (dto.OrganizerName is not null) tournament.OrganizerName = dto.OrganizerName;
        if (dto.MatchPointThreshold.HasValue) tournament.MatchPointThreshold = dto.MatchPointThreshold.Value;

        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }

    public async Task<Result<TournamentDto>> GetTournamentByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        var dto = _mapper.Map<TournamentDto>(tournament);
        dto.RegisteredTeamsCount = await _uow.TournamentTeams.CountAsync(tt => tt.TournamentId == id, ct);
        return Result.Success(dto);
    }

    public async Task<Result<PagedResult<TournamentListDto>>> GetTournamentsAsync(TournamentQueryDto query, CancellationToken ct = default)
    {
        var allTournaments = _uow.Tournaments.Query();

        if (query.Status.HasValue)
            allTournaments = allTournaments.Where(t => t.Status == query.Status.Value);
        if (query.Type.HasValue)
            allTournaments = allTournaments.Where(t => t.Type == query.Type.Value);
        if (query.Platform.HasValue)
            allTournaments = allTournaments.Where(t => t.Platform == query.Platform.Value);
        if (query.IsPrivate.HasValue)
            allTournaments = allTournaments.Where(t => t.IsPrivate == query.IsPrivate.Value);
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            allTournaments = allTournaments.Where(t => t.Name.Contains(query.SearchTerm));

        allTournaments = query.SortBy?.ToLower() switch
        {
            "name" => query.SortDescending ? allTournaments.OrderByDescending(t => t.Name) : allTournaments.OrderBy(t => t.Name),
            "startdate" => query.SortDescending ? allTournaments.OrderByDescending(t => t.StartDate) : allTournaments.OrderBy(t => t.StartDate),
            _ => query.SortDescending ? allTournaments.OrderByDescending(t => t.CreatedAt) : allTournaments.OrderBy(t => t.CreatedAt)
        };

        var totalCount = allTournaments.Count();
        var items = allTournaments
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<TournamentListDto>>(items);
        var pagedResult = new PagedResult<TournamentListDto>(dtos, totalCount, query.PageNumber, query.PageSize);
        return Result.Success(pagedResult);
    }

    public async Task<Result> DeleteTournamentAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure("Tournament not found.");

        if (tournament.Status == TournamentStatus.InProgress)
            return Result.Failure("Cannot delete a tournament that is in progress.");

        _uow.Tournaments.Remove(tournament);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<TournamentDto>> OpenRegistrationAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status != TournamentStatus.Draft)
            return Result.Failure<TournamentDto>("Tournament must be in Draft to open registration.");

        tournament.Status = TournamentStatus.Registration;
        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyTournamentStatusChangedAsync(id, TournamentStatus.Registration.ToString(), ct);
        await _discord.SendTournamentAnnouncementAsync(id, "📋 ¡Las inscripciones están abiertas! Regístrate ahora.", ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }

    public async Task<Result<TournamentDto>> StartCheckInAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status != TournamentStatus.Registration)
            return Result.Failure<TournamentDto>("Tournament must be in Registration status to start check-in.");

        tournament.Status = TournamentStatus.CheckIn;
        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyTournamentStatusChangedAsync(id, TournamentStatus.CheckIn.ToString(), ct);
        await _discord.SendTournamentAnnouncementAsync(id, "✅ ¡El check-in está abierto! Confirma tu asistencia.", ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }

    public async Task<Result<TournamentDto>> StartTournamentAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status != TournamentStatus.CheckIn && tournament.Status != TournamentStatus.Registration)
            return Result.Failure<TournamentDto>("Tournament must be in CheckIn or Registration status to start.");

        var checkedInTeams = await _uow.TournamentTeams.CountAsync(tt => tt.TournamentId == id && tt.CheckedIn, ct);
        if (checkedInTeams < 2)
            return Result.Failure<TournamentDto>("At least 2 teams must be checked in to start the tournament.");

        tournament.Status = TournamentStatus.InProgress;
        tournament.StartDate = DateTime.UtcNow;
        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyTournamentStatusChangedAsync(id, TournamentStatus.InProgress.ToString(), ct);
        await _discord.SendTournamentAnnouncementAsync(id, "🎮 ¡El torneo ha comenzado! ¡Buena suerte a todos!", ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }

    public async Task<Result<TournamentDto>> CompleteTournamentAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status != TournamentStatus.InProgress)
            return Result.Failure<TournamentDto>("Tournament must be in progress to complete.");

        tournament.Status = TournamentStatus.Completed;
        tournament.EndDate = DateTime.UtcNow;
        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyTournamentStatusChangedAsync(id, TournamentStatus.Completed.ToString(), ct);
        await _discord.SendTournamentAnnouncementAsync(id, "🏆 ¡El torneo ha finalizado! Gracias a todos los participantes.", ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }

    public async Task<Result<TournamentDto>> CancelTournamentAsync(Guid id, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(id, ct);
        if (tournament is null)
            return Result.Failure<TournamentDto>("Tournament not found.");

        if (tournament.Status == TournamentStatus.Completed)
            return Result.Failure<TournamentDto>("Cannot cancel a completed tournament.");

        tournament.Status = TournamentStatus.Cancelled;
        _uow.Tournaments.Update(tournament);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyTournamentStatusChangedAsync(id, TournamentStatus.Cancelled.ToString(), ct);
        await _discord.SendTournamentAnnouncementAsync(id, "❌ El torneo ha sido cancelado.", ct);

        return Result.Success(_mapper.Map<TournamentDto>(tournament));
    }
}
