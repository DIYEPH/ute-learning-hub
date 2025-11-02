using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable(DbTableNames.Comment);

        builder.HasKey(e => e.Id);

        builder.Property(u => u.ParentId).HasColumnName("RootId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");

        builder.ApplySoftDelete<Comment, Guid>()
            .ApplyTrack<Comment>()
            .ApplyAudit<Comment, Guid>()
            .ApplyReview<Comment, Guid>();
    }
}
