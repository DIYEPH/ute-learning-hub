using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;
public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable(DbTableNames.Tag);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.TagName).HasColumnName("TenTag");

        builder.ApplySoftDelete<Tag>()
            .ApplyTrack<Tag>()
            .ApplyAudit<Tag>()
            .ApplyReview<Tag>();
    }
}
