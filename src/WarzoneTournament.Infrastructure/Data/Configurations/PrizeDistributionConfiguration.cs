using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class PrizeDistributionConfiguration : IEntityTypeConfiguration<PrizeDistribution>
{
    public void Configure(EntityTypeBuilder<PrizeDistribution> builder)
    {
        builder.ToTable("PrizeDistributions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(200);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.HasIndex(p => new { p.TournamentId, p.Position }).IsUnique();
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
