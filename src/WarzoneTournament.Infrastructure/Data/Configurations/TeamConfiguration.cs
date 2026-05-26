using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Tag)
            .HasMaxLength(10);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.Country)
            .HasMaxLength(100);

        builder.Property(t => t.DiscordRoleId)
            .HasMaxLength(30);

        builder.Property(t => t.ContactEmail)
            .HasMaxLength(200);

        builder.Property(t => t.PreferredPlatform)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(t => t.Name);
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.Captain)
            .WithMany()
            .HasForeignKey(t => t.CaptainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TeamPlayers)
            .WithOne(tp => tp.Team)
            .HasForeignKey(tp => tp.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TournamentTeams)
            .WithOne(tt => tt.Team)
            .HasForeignKey(tt => tt.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.MatchResults)
            .WithOne(mr => mr.Team)
            .HasForeignKey(mr => mr.TeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
