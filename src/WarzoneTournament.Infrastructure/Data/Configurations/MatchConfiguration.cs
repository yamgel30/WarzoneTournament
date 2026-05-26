using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Status)
            .HasConversion<int>()
            .HasDefaultValue(MatchStatus.Pending);

        builder.Property(m => m.LobbyCode)
            .HasMaxLength(50);

        builder.Property(m => m.LobbyPassword)
            .HasMaxLength(100);

        builder.Property(m => m.MapName)
            .HasMaxLength(100);

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.RoundId);
        builder.HasIndex(m => m.TournamentId);
        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.HasMany(m => m.TeamResults)
            .WithOne(r => r.Match)
            .HasForeignKey(r => r.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.PlayerStats)
            .WithOne(s => s.Match)
            .HasForeignKey(s => s.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Evidences)
            .WithOne(e => e.Match)
            .HasForeignKey(e => e.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
