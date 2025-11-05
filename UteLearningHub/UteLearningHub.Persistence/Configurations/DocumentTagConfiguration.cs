using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentTagConfiguration : IEntityTypeConfiguration<DocumentTag>
{
    public void Configure(EntityTypeBuilder<DocumentTag> builder)
    {
        builder.ToTable(DbTableNames.DocumentTag);

        builder.HasKey(u => new { u.TagId, u.DocumentId });

        builder.Property(u => u.TagId).HasColumnName("TheId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");

        builder.HasOne(u => u.Tag)
            .WithMany(u => u.DocumentTags)
            .HasForeignKey(u => u.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Document)
            .WithMany(u => u.DocumentTags)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
