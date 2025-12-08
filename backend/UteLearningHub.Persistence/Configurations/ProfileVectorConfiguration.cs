using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class ProfileVectorConfiguration : IEntityTypeConfiguration<ProfileVector>
{
    public void Configure(EntityTypeBuilder<ProfileVector> builder)
    {
        builder.ToTable(DbTableNames.ProfileVector);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).HasColumnName("NguoiDungId").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnName("GiaTriVector").IsRequired();
        builder.Property(x => x.CalculatedAt).HasColumnName("ThoiDiemTinhToan").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("ConHieuLuc").HasDefaultValue(true);

        // Indexes cho performance
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);
    }
}