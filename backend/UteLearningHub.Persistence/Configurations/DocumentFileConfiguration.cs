using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentFileConfiguration : IEntityTypeConfiguration<DocumentFile>
{
    public void Configure(EntityTypeBuilder<DocumentFile> builder)
    {
        builder.ToTable(DbTableNames.DocumentFile);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.FileId).HasColumnName("TepDinhKemId");
        builder.Property(u => u.Title).HasColumnName("TieuDe");
        builder.Property(u => u.TotalPages).HasColumnName("TongSoTrang");
        builder.Property(u => u.Order).HasColumnName("ThuTu");
        builder.Property(u => u.CoverFileId).HasColumnName("TepBiaId");
        builder.Property(u => u.Status).HasColumnName("TrangThai");

        // Review info
        builder.Property(u => u.ReviewedById).HasColumnName("NguoiDuyetId");
        builder.Property(u => u.ReviewedAt).HasColumnName("ThoiGianDuyet");
        builder.Property(u => u.ReviewNote).HasColumnName("GhiChuDuyet");

        builder.ApplySoftDelete<DocumentFile>()
            .ApplyTrack<DocumentFile>()
            .ApplyAudit<DocumentFile>();

        builder.HasQueryFilter(u => !u.IsDeleted && !u.Document.IsDeleted);

        builder.HasOne(u => u.Document)
            .WithMany(u => u.DocumentFiles)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.File)
            .WithMany()
            .HasForeignKey(u => u.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.CoverFile)
            .WithMany()
            .HasForeignKey(u => u.CoverFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
