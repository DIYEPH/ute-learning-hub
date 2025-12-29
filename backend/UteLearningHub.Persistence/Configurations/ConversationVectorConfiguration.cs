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
        builder.Property(x => x.EmbeddingJson).HasColumnName("GiaTriVector").IsRequired();
        builder.Property(x => x.CalculatedAt).HasColumnName("ThoiDiemTinhToan").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("ConHieuLuc").HasDefaultValue(true);

        // Indexes
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter: exclude vectors of deleted conversations
        builder.HasQueryFilter(x => !x.Conversation.IsDeleted);
    }
}