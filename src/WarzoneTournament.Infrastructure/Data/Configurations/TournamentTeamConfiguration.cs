using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class TournamentTeamConfiguration : IEntityTypeConfiguration<TournamentTeam>
{
    public void Configure(EntityTypeBuilder<TournamentTeam> builder)
    {
        builder.ToTable("TournamentTeams");
        builder.HasKey(tt => tt.Id);

        builder.HasIndex(tt => new { tt.TournamentId, tt.TeamId }).IsUnique();
        builder.HasIndex(tt => tt.TournamentId);
        builder.HasQueryFilter(tt => !tt.IsDeleted);
    }
}
