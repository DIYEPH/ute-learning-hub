using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

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
        builder.Property(u => u.IsSuggestedByAI).HasColumnName("CoDuocTaoBoiAI");
        builder.Property(u => u.IsAllowMemberPin).HasColumnName("CoChoThanhVienGhimTinNhan");
        builder.Property(u => u.ConversationType).HasColumnName("LoaiCuocTroChuyen");
        builder.Property(u => u.ConversationStatus).HasColumnName("TrangThai");

        builder.ApplySoftDelete<Conversation>()
            .ApplyTrack<Conversation>()
            .ApplyAudit<Conversation>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Conversations)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Subject)
            .WithMany(u => u.Conversations)
            .HasForeignKey(u => u.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
