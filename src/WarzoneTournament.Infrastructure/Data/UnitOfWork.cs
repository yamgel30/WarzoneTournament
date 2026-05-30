using Microsoft.EntityFrameworkCore.Storage;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Interfaces;
using WarzoneTournament.Infrastructure.Data.Repositories;

namespace WarzoneTournament.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Tournament>? _tournaments;
    private IRepository<Team>? _teams;
    private IRepository<Player>? _players;
    private IRepository<Match>? _matches;
    private IRepository<Round>? _rounds;
    private IRepository<MatchTeamResult>? _matchTeamResults;
    private IRepository<PlayerMatchStats>? _playerMatchStats;
    private IRepository<TournamentRule>? _tournamentRules;
    private IRepository<PrizeDistribution>? _prizeDistributions;
    private IRepository<MatchEvidence>? _matchEvidences;
    private IRepository<EvidenceReview>? _evidenceReviews;
    private IRepository<OCRExtractionResult>? _ocrExtractionResults;
    private IRepository<TeamPlayer>? _teamPlayers;
    private IRepository<TournamentTeam>? _tournamentTeams;
    private IRepository<SiteSettings>? _siteSettings;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Tournament> Tournaments
        => _tournaments ??= new Repository<Tournament>(_context);

    public IRepository<Team> Teams
        => _teams ??= new Repository<Team>(_context);

    public IRepository<Player> Players
        => _players ??= new Repository<Player>(_context);

    public IRepository<Match> Matches
        => _matches ??= new Repository<Match>(_context);

    public IRepository<Round> Rounds
        => _rounds ??= new Repository<Round>(_context);

    public IRepository<MatchTeamResult> MatchTeamResults
        => _matchTeamResults ??= new Repository<MatchTeamResult>(_context);

    public IRepository<PlayerMatchStats> PlayerMatchStats
        => _playerMatchStats ??= new Repository<PlayerMatchStats>(_context);

    public IRepository<TournamentRule> TournamentRules
        => _tournamentRules ??= new Repository<TournamentRule>(_context);

    public IRepository<PrizeDistribution> PrizeDistributions
        => _prizeDistributions ??= new Repository<PrizeDistribution>(_context);

    public IRepository<MatchEvidence> MatchEvidences
        => _matchEvidences ??= new Repository<MatchEvidence>(_context);

    public IRepository<EvidenceReview> EvidenceReviews
        => _evidenceReviews ??= new Repository<EvidenceReview>(_context);

    public IRepository<OCRExtractionResult> OCRExtractionResults
        => _ocrExtractionResults ??= new Repository<OCRExtractionResult>(_context);

    public IRepository<TeamPlayer> TeamPlayers
        => _teamPlayers ??= new Repository<TeamPlayer>(_context);

    public IRepository<TournamentTeam> TournamentTeams
        => _tournamentTeams ??= new Repository<TournamentTeam>(_context);

    public IRepository<SiteSettings> SiteSettings
        => _siteSettings ??= new Repository<SiteSettings>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        // Detach all tracked entities so stale Added/Modified/Deleted state
        // from the failed transaction doesn't bleed into the next SaveChangesAsync.
        foreach (var entry in _context.ChangeTracker.Entries().ToList())
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
