using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class PlayerMatchStatsConfiguration : IEntityTypeConfiguration<PlayerMatchStats>
{
    public void Configure(EntityTypeBuilder<PlayerMatchStats> builder)
    {
        builder.ToTable("PlayerMatchStats");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.HeadshotPercentage)
            .HasPrecision(5, 2);

        builder.HasIndex(s => new { s.MatchId, s.PlayerId }).IsUnique();
        builder.HasIndex(s => s.PlayerId);
        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(s => s.Team)
            .WithMany()
            .HasForeignKey(s => s.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
