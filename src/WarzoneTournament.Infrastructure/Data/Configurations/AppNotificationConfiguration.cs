using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarzoneTournament.Domain.Entities;

namespace WarzoneTournament.Infrastructure.Data.Configurations;

public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.ToTable("AppNotifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(500);
        builder.Property(n => n.Link).HasMaxLength(300);
        builder.Property(n => n.Type).IsRequired().HasMaxLength(50);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasQueryFilter(n => !n.IsDeleted);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
