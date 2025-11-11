using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable(DbTableNames.Faculty);

        builder.HasKey(t => t.Id);
        builder.Property(u => u.FacultyName).HasColumnName("TenKhoa");
        builder.Property(u => u.FacultyCode).HasColumnName("MaKhoa");

        builder.ApplySoftDelete<Faculty>()
            .ApplyTrack<Faculty>()
            .ApplyAudit<Faculty>();
    }
}
