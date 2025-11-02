using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable(DbTableNames.Report);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.CommentId).HasColumnType("BinhLuanId");
        builder.Property(u => u.Content).HasColumnType("NoiDung");

        builder.ApplySoftDelete<Report, Guid>()
            .ApplyTrack<Report>()
            .ApplyAudit<Report, Guid>()
            .ApplyReview<Report, Guid>();
            
    }
}
