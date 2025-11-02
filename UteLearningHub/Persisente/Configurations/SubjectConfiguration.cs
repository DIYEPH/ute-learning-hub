using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;
public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable(DbTableNames.Subject);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.MajorId).HasColumnName("NganhId");
        builder.Property(u => u.SubjectName).HasColumnType("TenMonHoc");
        builder.Property(u => u.SubjectCode).HasColumnType("MaMonHoc");

        builder.ApplySoftDelete<Subject, Guid>()
            .ApplyTrack<Subject>()
            .ApplyAudit<Subject, Guid>()
            .ApplyReview<Subject, Guid>();
    }
}
