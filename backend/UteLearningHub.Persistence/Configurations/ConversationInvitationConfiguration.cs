using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationInvitationConfiguration : IEntityTypeConfiguration<ConversationInvitation>
{
    public void Configure(EntityTypeBuilder<ConversationInvitation> builder)
    {
        builder.ToTable(DbTableNames.ConversationInvitation);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(e => e.InvitedUserId).HasColumnName("NguoiDuocMoiId");
        builder.Property(e => e.Message).HasColumnName("LoiNhan").HasMaxLength(500);
        builder.Property(e => e.Status).HasColumnName("TrangThai");
        builder.Property(e => e.RespondedAt).HasColumnName("ThoiGianPhanHoi");
        builder.Property(e => e.ResponseNote).HasColumnName("GhiChuPhanHoi").HasMaxLength(500);

        builder.ApplySoftDelete<ConversationInvitation>()
            .ApplyTrack<ConversationInvitation>()
            .ApplyAudit<ConversationInvitation>();

        // Người gửi lời mời
        builder.HasOne<AppUser>()
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Người được mời
        builder.HasOne<AppUser>()
            .WithMany(u => u.ReceivedInvitations)
            .HasForeignKey(e => e.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Conversation
        builder.HasOne(e => e.Conversation)
            .WithMany()
            .HasForeignKey(e => e.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
