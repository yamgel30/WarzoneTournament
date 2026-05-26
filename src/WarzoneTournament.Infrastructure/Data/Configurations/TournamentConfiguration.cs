using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(TournamentStatus.Draft);

        builder.Property(t => t.Platform)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.PrizePool)
            .HasPrecision(18, 2);

        builder.Property(t => t.PlacementPointsJson)
            .HasMaxLength(2000)
            .HasDefaultValue("{}");

        builder.Property(t => t.TournamentRulesText)
            .HasMaxLength(10000);

        builder.Property(t => t.StreamUrl)
            .HasMaxLength(500);

        builder.Property(t => t.DiscordChannelId)
            .HasMaxLength(30);

        builder.Property(t => t.DiscordGuildId)
            .HasMaxLength(30);

        builder.Property(t => t.LobbyCode)
            .HasMaxLength(50);

        builder.Property(t => t.LobbyPassword)
            .HasMaxLength(100);

        builder.Property(t => t.BannerImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.OrganizerName)
            .HasMaxLength(200);

        builder.Property(t => t.KillPoints)
            .HasDefaultValue(1);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.CreatedAt);

        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasMany(t => t.TournamentTeams)
            .WithOne(tt => tt.Tournament)
            .HasForeignKey(tt => tt.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Rounds)
            .WithOne(r => r.Tournament)
            .HasForeignKey(r => r.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TournamentRules)
            .WithOne(r => r.Tournament)
            .HasForeignKey(r => r.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.PrizeDistributions)
            .WithOne(p => p.Tournament)
            .HasForeignKey(p => p.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Matches)
            .WithOne(m => m.Tournament)
            .HasForeignKey(m => m.TournamentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
