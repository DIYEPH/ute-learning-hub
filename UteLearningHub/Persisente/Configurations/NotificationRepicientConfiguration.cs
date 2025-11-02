using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class NotificationRepicientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable(DbTableNames.NotificationRecipient);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.NotificationId).HasColumnName("ThongBaoId");
        builder.Property(u => u.RecipientId).HasColumnName("NguoiNhanId");
        builder.Property(u => u.IsSent).HasColumnName("CoDaGui");
        builder.Property(u => u.IsRead).HasColumnName("CoDaDoc");

        builder.ApplySoftDelete<NotificationRecipient, Guid>()
            .ApplyTrack<NotificationRecipient>();
    }
}
