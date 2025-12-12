using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable(DbTableNames.Report);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentFileId).HasColumnName("TaiLieu_TepId");
        builder.Property(u => u.CommentId).HasColumnName("BinhLuanId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");
        builder.Property(u => u.Status).HasColumnName("TrangThai");
        builder.Property(u => u.ReviewedById).HasColumnName("DuyetBoiId");
        builder.Property(u => u.ReviewedAt).HasColumnName("DuyetLuc");
        builder.Property(u => u.ReviewNote).HasColumnName("GhiChuDuyet");

        builder.ApplySoftDelete<Report>()
            .ApplyTrack<Report>()
            .ApplyAudit<Report>();


        builder.HasOne<AppUser>()
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany(u => u.ReviewedReports)
            .HasForeignKey(u => u.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.DocumentFile)
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.DocumentFileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Comment)
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.CommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
