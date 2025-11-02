using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        builder.ToTable(DbTableNames.Major);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.MajorName).HasColumnName("TenNganh");
        builder.Property(u => u.MajorCode).HasColumnName("MaNganh");

        builder.ApplySoftDelete<Major, Guid>()
            .ApplyTrack<Major>()
            .ApplyAudit<Major, Guid>()
            .ApplyReview<Major, Guid>();
    }
}
