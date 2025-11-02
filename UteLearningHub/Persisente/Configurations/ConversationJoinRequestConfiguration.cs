using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class ConversationJoinRequestConfiguration : IEntityTypeConfiguration<ConversationJoinRequest>
{
    public void Configure(EntityTypeBuilder<ConversationJoinRequest> builder)
    {
        builder.ToTable(DbTableNames.ConversationJoinRequest);

        builder.HasKey(e => e.Id);
        builder.Property(u => u.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");

        builder.ApplySoftDelete<ConversationJoinRequest, Guid>()
            .ApplyTrack<ConversationJoinRequest>()
            .ApplyAudit<ConversationJoinRequest, Guid>()
            .ApplyReview<ConversationJoinRequest, Guid>();
    }
}
