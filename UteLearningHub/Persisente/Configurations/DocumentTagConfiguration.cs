using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentTagConfiguration : IEntityTypeConfiguration<DocumentTag>
{
    public void Configure(EntityTypeBuilder<DocumentTag> builder)
    {
        builder.ToTable(DbTableNames.DocumentTag);

        builder.HasKey(u => new { u.TagId, u.DocumentId });

        builder.Property(u => u.TagId).HasColumnName("TheId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
    }
}
