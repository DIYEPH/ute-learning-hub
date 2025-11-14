using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable(DbTableNames.Notification);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.ObjectId).HasColumnName("DoiTuongId");
        builder.Property(u => u.Title).HasColumnName("TieuDe");
        builder.Property(u => u.Content).HasColumnName("NoiDung");
        builder.Property(u => u.Link).HasColumnName("Link");
        builder.Property(u => u.IsGlobal).HasColumnName("CoToanBo");
        builder.Property(u => u.ExpiredAt).HasColumnName("ThoiGianHetHan");
        builder.Property(u => u.NotificationType).HasColumnName("LoaiThongBao");
        builder.Property(u => u.NotificationPriorityType).HasColumnName("CapDoThongBao");

        builder.ApplySoftDelete<Notification>()
            .ApplyTrack<Notification>()
            .ApplyAudit<Notification>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.CreatedNotifications)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
