using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ActivisionId)
            .HasMaxLength(100);

        builder.Property(p => p.DiscordId)
            .HasMaxLength(30);

        builder.Property(p => p.DiscordUsername)
            .HasMaxLength(100);

        builder.Property(p => p.Email)
            .HasMaxLength(200);

        builder.Property(p => p.Country)
            .HasMaxLength(100);

        builder.Property(p => p.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(p => p.BanReason)
            .HasMaxLength(500);

        builder.Property(p => p.Platform)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(p => p.Username);
        builder.HasIndex(p => p.DiscordId).IsUnique().HasFilter("[DiscordId] IS NOT NULL");
        builder.HasIndex(p => p.ActivisionId);
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.TeamPlayers)
            .WithOne(tp => tp.Player)
            .HasForeignKey(tp => tp.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.MatchStats)
            .WithOne(ms => ms.Player)
            .HasForeignKey(ms => ms.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
