using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class TeamPlayerConfiguration : IEntityTypeConfiguration<TeamPlayer>
{
    public void Configure(EntityTypeBuilder<TeamPlayer> builder)
    {
        builder.ToTable("TeamPlayers");
        builder.HasKey(tp => tp.Id);

        builder.Property(tp => tp.Role)
            .HasMaxLength(50);

        builder.HasIndex(tp => new { tp.TeamId, tp.PlayerId });
        builder.HasQueryFilter(tp => !tp.IsDeleted);
    }
}
