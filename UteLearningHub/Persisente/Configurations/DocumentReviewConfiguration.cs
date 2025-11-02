using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class DocumentReviewConfiguration : IEntityTypeConfiguration<DocumentReview>
{
    public void Configure(EntityTypeBuilder<DocumentReview> builder)
    {
        builder.ToTable(DbTableNames.DocumentReview);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.DocumentReviewType).HasColumnName("LoaiDanhGia");

        builder.ApplySoftDelete<DocumentReview, Guid>()
            .ApplyTrack<DocumentReview>()
            .ApplyAudit<DocumentReview, Guid>();
    }
}
