using AutoMapper;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Evidence;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class EvidenceService : IEvidenceService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<EvidenceService> _logger;
    private readonly ISignalRNotificationService _signalR;
    private readonly IDiscordNotificationService _discord;

    public EvidenceService(IUnitOfWork uow, IMapper mapper,
        ILogger<EvidenceService> logger, ISignalRNotificationService signalR,
        IDiscordNotificationService discord)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _signalR = signalR;
        _discord = discord;
    }

    public async Task<Result<EvidenceDto>> SubmitEvidenceAsync(SubmitEvidenceDto dto, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(dto.MatchId, ct);
        if (match is null) return Result.Failure<EvidenceDto>("Match not found.");

        if (match.Status == Domain.Enums.MatchStatus.Completed || match.Status == Domain.Enums.MatchStatus.Cancelled)
            return Result.Failure<EvidenceDto>("Cannot submit evidence for a completed or cancelled match.");

        var team = await _uow.Teams.GetByIdAsync(dto.SubmittedByTeamId, ct);
        if (team is null) return Result.Failure<EvidenceDto>("Team not found.");

        var evidence = new MatchEvidence
        {
            MatchId = dto.MatchId,
            SubmittedByTeamId = dto.SubmittedByTeamId,
            SubmittedByPlayerId = dto.SubmittedByPlayerId,
            ImageUrl = dto.ImageUrl,
            OriginalFileName = dto.OriginalFileName,
            FileSizeBytes = dto.FileSizeBytes,
            MimeType = dto.MimeType,
            SourceType = dto.SourceType,
            Status = EvidenceStatus.Pending
        };

        await _uow.MatchEvidences.AddAsync(evidence, ct);
        await _uow.SaveChangesAsync(ct);

        await _signalR.NotifyEvidenceSubmittedAsync(dto.MatchId, evidence.Id, ct);

        _logger.LogInformation("Evidence submitted for match {MatchId} by team {TeamId}", dto.MatchId, dto.SubmittedByTeamId);
        return Result.Success(await BuildEvidenceDtoAsync(evidence, ct));
    }

    public async Task<Result<EvidenceDto>> GetEvidenceByIdAsync(Guid id, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(id, ct);
        if (evidence is null) return Result.Failure<EvidenceDto>("Evidence not found.");

        return Result.Success(await BuildEvidenceDtoAsync(evidence, ct));
    }

    public async Task<Result<IReadOnlyList<EvidenceDto>>> GetEvidenceByMatchAsync(Guid matchId, CancellationToken ct = default)
    {
        var evidences = await _uow.MatchEvidences.FindAsync(e => e.MatchId == matchId, ct);
        var dtos = new List<EvidenceDto>();
        foreach (var e in evidences.OrderByDescending(x => x.CreatedAt))
            dtos.Add(await BuildEvidenceDtoAsync(e, ct));

        return Result.Success<IReadOnlyList<EvidenceDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<EvidenceDto>>> GetPendingEvidenceAsync(CancellationToken ct = default)
    {
        var evidences = await _uow.MatchEvidences.FindAsync(
            e => e.Status == EvidenceStatus.Pending || e.Status == EvidenceStatus.UnderReview, ct);

        var dtos = new List<EvidenceDto>();
        foreach (var e in evidences.OrderBy(x => x.CreatedAt))
            dtos.Add(await BuildEvidenceDtoAsync(e, ct));

        return Result.Success<IReadOnlyList<EvidenceDto>>(dtos);
    }

    public async Task<Result<EvidenceDto>> ApproveEvidenceAsync(Guid id, string reviewedBy, string? notes = null, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(id, ct);
        if (evidence is null) return Result.Failure<EvidenceDto>("Evidence not found.");

        if (evidence.Status == EvidenceStatus.Approved)
            return Result.Failure<EvidenceDto>("Evidence is already approved.");

        evidence.Status = EvidenceStatus.Approved;
        _uow.MatchEvidences.Update(evidence);

        await _uow.EvidenceReviews.AddAsync(new EvidenceReview
        {
            EvidenceId = id,
            ReviewedBy = reviewedBy,
            Decision = EvidenceStatus.Approved,
            Notes = notes,
            ReviewedAt = DateTime.UtcNow
        }, ct);

        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyEvidenceReviewedAsync(id, "Approved", ct);

        _logger.LogInformation("Evidence {EvidenceId} approved by {ReviewedBy}", id, reviewedBy);
        return Result.Success(await BuildEvidenceDtoAsync(evidence, ct));
    }

    public async Task<Result<EvidenceDto>> RejectEvidenceAsync(Guid id, string reviewedBy, string reason, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(id, ct);
        if (evidence is null) return Result.Failure<EvidenceDto>("Evidence not found.");

        evidence.Status = EvidenceStatus.Rejected;
        evidence.RejectionReason = reason;
        _uow.MatchEvidences.Update(evidence);

        await _uow.EvidenceReviews.AddAsync(new EvidenceReview
        {
            EvidenceId = id,
            ReviewedBy = reviewedBy,
            Decision = EvidenceStatus.Rejected,
            Notes = reason,
            ReviewedAt = DateTime.UtcNow
        }, ct);

        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyEvidenceReviewedAsync(id, "Rejected", ct);
        await _discord.SendEvidenceRejectionNotificationAsync(id, reason, ct);

        _logger.LogInformation("Evidence {EvidenceId} rejected by {ReviewedBy}: {Reason}", id, reviewedBy, reason);
        return Result.Success(await BuildEvidenceDtoAsync(evidence, ct));
    }

    public async Task<Result<EvidenceDto>> SubmitEvidenceFromDiscordAsync(DiscordEvidenceDto dto, CancellationToken ct = default)
    {
        var duplicate = await _uow.MatchEvidences.ExistsAsync(
            e => e.DiscordMessageId == dto.DiscordMessageId, ct);
        if (duplicate)
            return Result.Failure<EvidenceDto>("This Discord message has already been submitted as evidence.");

        var evidence = new MatchEvidence
        {
            MatchId = dto.MatchId,
            SubmittedByTeamId = dto.SubmittedByTeamId,
            ImageUrl = dto.ImageUrl,
            DiscordMessageId = dto.DiscordMessageId,
            DiscordChannelId = dto.DiscordChannelId,
            OriginalFileName = dto.OriginalFileName,
            FileSizeBytes = dto.FileSizeBytes,
            SourceType = EvidenceSourceType.DiscordBot,
            Status = EvidenceStatus.Pending
        };

        await _uow.MatchEvidences.AddAsync(evidence, ct);
        await _uow.SaveChangesAsync(ct);

        await _signalR.NotifyEvidenceSubmittedAsync(dto.MatchId, evidence.Id, ct);

        _logger.LogInformation("Discord evidence submitted for match {MatchId}, message {MessageId}",
            dto.MatchId, dto.DiscordMessageId);

        return Result.Success(await BuildEvidenceDtoAsync(evidence, ct));
    }

    private async Task<EvidenceDto> BuildEvidenceDtoAsync(MatchEvidence evidence, CancellationToken ct)
    {
        var team = await _uow.Teams.GetByIdAsync(evidence.SubmittedByTeamId, ct);
        Player? player = null;
        if (evidence.SubmittedByPlayerId.HasValue)
            player = await _uow.Players.GetByIdAsync(evidence.SubmittedByPlayerId.Value, ct);

        var reviews = await _uow.EvidenceReviews.FindAsync(r => r.EvidenceId == evidence.Id, ct);
        var ocrResult = await _uow.OCRExtractionResults.FirstOrDefaultAsync(o => o.EvidenceId == evidence.Id, ct);

        return new EvidenceDto
        {
            Id = evidence.Id,
            MatchId = evidence.MatchId,
            SubmittedByTeamId = evidence.SubmittedByTeamId,
            SubmittedByTeamName = team?.Name,
            SubmittedByPlayerId = evidence.SubmittedByPlayerId,
            SubmittedByPlayerUsername = player?.Username,
            ImageUrl = evidence.ImageUrl,
            ThumbnailUrl = evidence.ThumbnailUrl,
            Status = evidence.Status,
            SourceType = evidence.SourceType,
            DiscordMessageId = evidence.DiscordMessageId,
            OriginalFileName = evidence.OriginalFileName,
            FileSizeBytes = evidence.FileSizeBytes,
            OcrProcessed = evidence.OcrProcessed,
            RejectionReason = evidence.RejectionReason,
            OcrResult = ocrResult is not null ? new OcrResultDto
            {
                EvidenceId = ocrResult.EvidenceId,
                RawText = ocrResult.RawText,
                ExtractedPlacement = ocrResult.ExtractedPlacement,
                ExtractedKills = ocrResult.ExtractedKills,
                ExtractedTeamName = ocrResult.ExtractedTeamName,
                ConfidenceScore = ocrResult.ConfidenceScore,
                RequiresManualReview = ocrResult.RequiresManualReview,
                ProcessingError = ocrResult.ProcessingError,
                ProcessedAt = ocrResult.ProcessedAt,
                OcrProvider = ocrResult.OcrProvider
            } : null,
            Reviews = reviews.Select(r => new EvidenceReviewDto
            {
                Id = r.Id,
                ReviewedBy = r.ReviewedBy,
                Decision = r.Decision,
                Notes = r.Notes,
                ReviewedAt = r.ReviewedAt
            }).ToList(),
            CreatedAt = evidence.CreatedAt
        };
    }
}
