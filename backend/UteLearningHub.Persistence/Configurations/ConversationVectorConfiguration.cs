using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationVectorConfiguration : IEntityTypeConfiguration<ConversationVector>
{
    public void Configure(EntityTypeBuilder<ConversationVector> builder)
    {
        builder.ToTable(DbTableNames.ConversationVector);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(x => x.SubjectId).HasColumnName("MonHocId");
        builder.Property(x => x.VectorType).HasColumnName("LoaiVector");
        builder.Property(x => x.VectorDimension).HasColumnName("SoChieu");
        builder.Property(x => x.EmbeddingJson).HasColumnName("GiaTriVector");
        builder.Property(x => x.SourceDataJson).HasColumnName("NguonDuLieu");
        builder.Property(x => x.ModelVersion).HasColumnName("PhienBanMoHinh");
        builder.Property(x => x.SimilarityMetric).HasColumnName("KieuDoTuongDong");
        builder.Property(x => x.CalculatedAt).HasColumnName("ThoiDiemTinhToan");
        builder.Property(x => x.IsActive).HasColumnName("ConHieuLuc");

        builder.ApplySoftDelete<ConversationVector>()
            .ApplyTrack<ConversationVector>();

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

