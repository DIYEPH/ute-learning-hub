using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Identity;

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
        builder.Property(u => u.ConversationMemberRoleType).HasColumnName("QuyenNhom");

        builder.ApplySoftDelete<ConversationMember>()
            .ApplyTrack<ConversationMember>();

        builder.HasOne(u => u.Conversation)
            .WithMany(u => u.Members)
            .HasForeignKey(u => u.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany(u => u.ConversationMembers)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
