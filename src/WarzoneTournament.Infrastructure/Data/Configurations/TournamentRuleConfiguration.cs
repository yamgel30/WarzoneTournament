using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class TournamentRuleConfiguration : IEntityTypeConfiguration<TournamentRule>
{
    public void Configure(EntityTypeBuilder<TournamentRule> builder)
    {
        builder.ToTable("TournamentRules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.HasIndex(r => new { r.TournamentId, r.Order });
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
