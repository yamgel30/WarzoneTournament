using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class PendingDiscordInviteConfiguration : IEntityTypeConfiguration<PendingDiscordInvite>
{
    public void Configure(EntityTypeBuilder<PendingDiscordInvite> builder)
    {
        builder.ToTable("PendingDiscordInvites");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.InvitedDiscordId).IsRequired().HasMaxLength(30);
        builder.Property(p => p.Message).HasMaxLength(500);

        builder.HasIndex(p => p.InvitedDiscordId);
        builder.HasIndex(p => new { p.TeamId, p.InvitedDiscordId });
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Team)
            .WithMany()
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.InvitedByPlayer)
            .WithMany()
            .HasForeignKey(p => p.InvitedByPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
