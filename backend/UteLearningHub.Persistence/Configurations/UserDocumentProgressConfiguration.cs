using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class UserDocumentProgressConfiguration : IEntityTypeConfiguration<UserDocumentProgress>
{
    public void Configure(EntityTypeBuilder<UserDocumentProgress> builder)
    {
        builder.ToTable(DbTableNames.UserDocumentProgress);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.DocumentFileId).HasColumnName("TaiLieu_TepId");
        builder.Property(u => u.LastPage).HasColumnName("TrangCuoiCung");
        builder.Property(u => u.TotalPages).HasColumnName("TongSoTrang");
        builder.Property(u => u.LastAccessedAt).HasColumnName("LanTruyCapGanNhat");

        builder.ApplyTrack<UserDocumentProgress>();

        builder.HasOne(u => u.Document)
            .WithMany(u => u.UserProgresses)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.DocumentFile)
            .WithMany()
            .HasForeignKey(u => u.DocumentFileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter: exclude progress of deleted documents
        builder.HasQueryFilter(u => !u.Document.IsDeleted);
    }
}


