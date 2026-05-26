using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class MatchEvidenceConfiguration : IEntityTypeConfiguration<MatchEvidence>
{
    public void Configure(EntityTypeBuilder<MatchEvidence> builder)
    {
        builder.ToTable("MatchEvidences");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.ThumbnailUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .HasDefaultValue(EvidenceStatus.Pending);

        builder.Property(e => e.SourceType)
            .HasConversion<int>()
            .HasDefaultValue(EvidenceSourceType.ManualUpload);

        builder.Property(e => e.DiscordMessageId)
            .HasMaxLength(30);

        builder.Property(e => e.DiscordChannelId)
            .HasMaxLength(30);

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(e => e.MimeType)
            .HasMaxLength(100);

        builder.Property(e => e.RejectionReason)
            .HasMaxLength(500);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.MatchId);
        builder.HasIndex(e => e.DiscordMessageId);
        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.SubmittedByTeam)
            .WithMany()
            .HasForeignKey(e => e.SubmittedByTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SubmittedByPlayer)
            .WithMany()
            .HasForeignKey(e => e.SubmittedByPlayerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Reviews)
            .WithOne(r => r.Evidence)
            .HasForeignKey(r => r.EvidenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.OcrResult)
            .WithOne(o => o.Evidence)
            .HasForeignKey<OCRExtractionResult>(o => o.EvidenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
