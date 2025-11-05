using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentFileConfiguration : IEntityTypeConfiguration<DocumentFile>
{
    public void Configure(EntityTypeBuilder<DocumentFile> builder)
    {
        builder.ToTable(DbTableNames.DocumentFile);

        builder.HasKey(u => new { u.FileId, u.DocumentId });

        builder.Property(u => u.FileId).HasColumnName("TepId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");

        builder.HasOne(u => u.Document)
            .WithMany(u => u.DocumentFiles)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.File)
            .WithMany(u => u.DocumentFiles)
            .HasForeignKey(u => u.FileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
