using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class OcrExtractionResultConfiguration : IEntityTypeConfiguration<OCRExtractionResult>
{
    public void Configure(EntityTypeBuilder<OCRExtractionResult> builder)
    {
        builder.ToTable("OCRExtractionResults");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.RawText)
            .HasMaxLength(5000);

        builder.Property(o => o.ExtractedTeamName)
            .HasMaxLength(100);

        builder.Property(o => o.ConfidenceScore)
            .HasPrecision(5, 4);

        builder.Property(o => o.ProcessingError)
            .HasMaxLength(500);

        builder.Property(o => o.OcrProvider)
            .HasMaxLength(50)
            .HasDefaultValue("Tesseract");

        builder.HasIndex(o => o.EvidenceId).IsUnique();
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
