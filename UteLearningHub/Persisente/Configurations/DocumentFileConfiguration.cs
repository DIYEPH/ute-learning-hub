using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class DocumentFileConfiguration : IEntityTypeConfiguration<DocumentFile>
{
    public void Configure(EntityTypeBuilder<DocumentFile> builder)
    {
        builder.ToTable(DbTableNames.DocumentFile);

        builder.HasKey(u => new { u.FileId, u.DocumentId });

        builder.Property(u => u.FileId).HasColumnName("TepId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
    }
}
