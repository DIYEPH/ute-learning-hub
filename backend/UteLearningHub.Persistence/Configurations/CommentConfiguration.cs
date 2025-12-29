using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable(DbTableNames.Comment);

        builder.HasKey(e => e.Id);

        builder.Property(u => u.ParentId).HasColumnName("PhanTuChaId");
        builder.Property(u => u.DocumentFileId).HasColumnName("TaiLieu_TepId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");
        builder.Property(u => u.Status).HasColumnName("TrangThai");

        // Review info
        builder.Property(u => u.ReviewedById).HasColumnName("NguoiDuyetId");
        builder.Property(u => u.ReviewedAt).HasColumnName("ThoiGianDuyet");
        builder.Property(u => u.ReviewNote).HasColumnName("GhiChuDuyet");

        builder.ApplySoftDelete<Comment>()
            .ApplyTrack<Comment>()
            .ApplyAudit<Comment>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Comments)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne<Comment>()
            .WithMany(u => u.Childrens)
            .HasForeignKey(u => u.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.DocumentFile)
            .WithMany(u => u.Comments)
            .HasForeignKey(u => u.DocumentFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
