using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationMemberConfiguration : IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> builder) 
    {
        builder.ToTable(DbTableNames.ConversationMember);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
        builder.Property(u => u.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(u => u.LastReadMessageId).HasColumnName("TinNhanDocGanNhat");
        builder.Property(u => u.IsMuted).HasColumnName("CoBiChanChat");
        builder.Property(u => u.ConversationMemberRoleType).HasColumnType("QuyenNhom");

        builder.ApplySoftDelete<ConversationMember, Guid>()
            .ApplyTrack<ConversationMember>();
    }
}
