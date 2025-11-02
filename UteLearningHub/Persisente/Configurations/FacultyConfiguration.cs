using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable(DbTableNames.Faculty);

        builder.HasKey(t => t.Id);
        builder.Property(u => u.FacultyName).HasColumnName("TenKhoa");
        builder.Property(u => u.FacultyCode).HasColumnName("MaKhoa");

        builder.ApplySoftDelete<Faculty, Guid>()
            .ApplyTrack<Faculty>()
            .ApplyAudit<Faculty, Guid>()
            .ApplyReview<Faculty, Guid>();
    }
}
