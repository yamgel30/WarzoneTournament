using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Tournament> Tournaments { get; }
    IRepository<Team> Teams { get; }
    IRepository<Player> Players { get; }
    IRepository<Match> Matches { get; }
    IRepository<Round> Rounds { get; }
    IRepository<MatchTeamResult> MatchTeamResults { get; }
    IRepository<PlayerMatchStats> PlayerMatchStats { get; }
    IRepository<TournamentRule> TournamentRules { get; }
    IRepository<PrizeDistribution> PrizeDistributions { get; }
    IRepository<MatchEvidence> MatchEvidences { get; }
    IRepository<EvidenceReview> EvidenceReviews { get; }
    IRepository<OCRExtractionResult> OCRExtractionResults { get; }
    IRepository<TeamPlayer> TeamPlayers { get; }
    IRepository<TournamentTeam> TournamentTeams { get; }
    IRepository<SiteSettings> SiteSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
