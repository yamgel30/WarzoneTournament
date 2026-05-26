using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<MatchTeamResult> MatchTeamResults => Set<MatchTeamResult>();
    public DbSet<PlayerMatchStats> PlayerMatchStats => Set<PlayerMatchStats>();
    public DbSet<TournamentRule> TournamentRules => Set<TournamentRule>();
    public DbSet<PrizeDistribution> PrizeDistributions => Set<PrizeDistribution>();
    public DbSet<MatchEvidence> MatchEvidences => Set<MatchEvidence>();
    public DbSet<EvidenceReview> EvidenceReviews => Set<EvidenceReview>();
    public DbSet<OCRExtractionResult> OCRExtractionResults => Set<OCRExtractionResult>();
    public DbSet<TeamPlayer> TeamPlayers => Set<TeamPlayer>();
    public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
