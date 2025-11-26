using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationVectorConfiguration : IEntityTypeConfiguration<ConversationVector>
{
    public void Configure(EntityTypeBuilder<ConversationVector> builder)
    {
        builder.ToTable(DbTableNames.ConversationVector);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).HasColumnName("CuocTroChuyenId").IsRequired();
        builder.Property(x => x.SubjectId).HasColumnName("MonHocId");
        builder.Property(x => x.VectorType).HasColumnName("LoaiVector").IsRequired();
        builder.Property(x => x.VectorDimension).HasColumnName("SoChieu").IsRequired();
        builder.Property(x => x.EmbeddingJson).HasColumnName("GiaTriVector").IsRequired();
        builder.Property(x => x.SourceDataJson).HasColumnName("NguonDuLieu");
        builder.Property(x => x.CalculatedAt).HasColumnName("ThoiDiemTinhToan").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("ConHieuLuc").HasDefaultValue(true);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}