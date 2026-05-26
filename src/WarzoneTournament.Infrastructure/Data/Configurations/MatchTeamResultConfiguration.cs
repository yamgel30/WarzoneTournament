using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class MatchTeamResultConfiguration : IEntityTypeConfiguration<MatchTeamResult>
{
    public void Configure(EntityTypeBuilder<MatchTeamResult> builder)
    {
        builder.ToTable("MatchTeamResults");
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.MatchId, r.TeamId }).IsUnique();
        builder.HasIndex(r => r.MatchId);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
