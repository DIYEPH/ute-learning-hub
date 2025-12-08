using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentReviewConfiguration : IEntityTypeConfiguration<DocumentReview>
{
    public void Configure(EntityTypeBuilder<DocumentReview> builder)
    {
        builder.ToTable(DbTableNames.DocumentReview);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.DocumentFileId).HasColumnName("TaiLieu_TepId");
        builder.Property(u => u.DocumentReviewType).HasColumnName("LoaiDanhGia");

        builder.ApplyTrack<DocumentReview>()
            .ApplyAudit<DocumentReview>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.DocumentReviews)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Document)
            .WithMany(u => u.Reviews)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.DocumentFile)
            .WithMany()
            .HasForeignKey(u => u.DocumentFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
