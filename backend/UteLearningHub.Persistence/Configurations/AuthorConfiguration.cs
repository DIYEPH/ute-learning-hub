using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable(DbTableNames.Author);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).HasColumnName("TenTacGia");
        builder.Property(u => u.Description).HasColumnName("MoTa");

        builder.ApplySoftDelete<Author>()
            .ApplyTrack<Author>()
            .ApplyAudit<Author>()
            .ApplyReview<Author>();
    }
}


