using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Rounds");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasIndex(r => new { r.TournamentId, r.RoundNumber });
        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.HasMany(r => r.Matches)
            .WithOne(m => m.Round)
            .HasForeignKey(m => m.RoundId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
