using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class UserTrustHistoryConfiguration : IEntityTypeConfiguration<UserTrustHistory>
{
    public void Configure(EntityTypeBuilder<UserTrustHistory> builder)
    {
        builder.ToTable(DbTableNames.UserTrustHistory);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
        builder.Property(u => u.Score).HasColumnName("DiemThayDoi");
        builder.Property(u => u.OldScore).HasColumnName("DiemCu");
        builder.Property(u => u.NewScore).HasColumnName("DiemMoi");
        builder.Property(u => u.Reason).HasColumnName("LyDo");
        builder.Property(u => u.EntityId).HasColumnName("ThucTheId");
        builder.Property(u => u.EntityType).HasColumnName("LoaiThucThe");

        builder.ApplyTrack<UserTrustHistory>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.UserTrustHistories)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
