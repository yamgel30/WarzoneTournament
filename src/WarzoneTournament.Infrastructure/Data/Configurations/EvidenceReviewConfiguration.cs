using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class EvidenceReviewConfiguration : IEntityTypeConfiguration<EvidenceReview>
{
    public void Configure(EntityTypeBuilder<EvidenceReview> builder)
    {
        builder.ToTable("EvidenceReviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Decision)
            .HasConversion<int>();

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(r => r.EvidenceId);
        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
