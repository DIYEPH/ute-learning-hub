using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class ProfileVectorConfiguration : IEntityTypeConfiguration<ProfileVector>
{
    public void Configure(EntityTypeBuilder<ProfileVector> builder)
    {
        builder.ToTable(DbTableNames.ProfileVector);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasColumnName("NguoiDungId").IsRequired();
        builder.Property(x => x.SubjectId).HasColumnName("MonHocId");
        builder.Property(x => x.VectorType).HasColumnName("LoaiVector").IsRequired();
        builder.Property(x => x.VectorDimension).HasColumnName("SoChieu").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnName("GiaTriVector").IsRequired();
        builder.Property(x => x.SourceDataJson).HasColumnName("NguonDuLieu");
        builder.Property(x => x.CalculatedAt).HasColumnName("ThoiDiemTinhToan").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("ConHieuLuc").HasDefaultValue(true);

        // Indexes cho performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.VectorType });
        builder.HasIndex(x => new { x.UserId, x.SubjectId });
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}