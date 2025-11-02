using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable(DbTableNames.Conversation);

        builder.HasKey(x => x.Id);

        builder.Property(u => u.SubjectId).HasColumnName("MonHocId");
        builder.Property(u => u.LastMessage).HasColumnName("TinNhanMoiNhat");
        builder.Property(u => u.ConversationName).HasColumnName("TenCuocTroChuyen");
        builder.Property(u => u.Topic).HasColumnName("ChuDe");
        builder.Property(u => u.IsSuggestedByAI).HasColumnName("CoDuocTaoBoiAI");
        builder.Property(u => u.IsAllowMemberPin).HasColumnName("CoChoThanhVienGhimTinNhan");

        builder.ApplySoftDelete<Conversation, Guid>()
            .ApplyTrack<Conversation>()
            .ApplyAudit<Conversation, Guid>();
    }
}
