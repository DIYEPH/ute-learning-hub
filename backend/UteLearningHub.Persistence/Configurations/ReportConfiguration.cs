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

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.CommentId).HasColumnName("BinhLuanId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");

        builder.ApplySoftDelete<Report>()
            .ApplyTrack<Report>()
            .ApplyAudit<Report>()
            .ApplyReview<Report>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany(u => u.ReviewedReports)
            .HasForeignKey(u => u.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Document)
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Comment)
            .WithMany(u => u.Reports)
            .HasForeignKey(u => u.CommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
