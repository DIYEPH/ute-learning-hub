using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable(DbTableNames.Message);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.ConversationId).HasColumnName("TenNganh");
        builder.Property(u => u.ParentId).HasColumnName("MaNganh");
        builder.Property(u => u.Content).HasColumnName("NoiDung");
        builder.Property(u => u.IsEdit).HasColumnName("CoChinhSua");
        builder.Property(u => u.IsPined).HasColumnName("CoDaGhim");

        builder.ApplySoftDelete<Message, Guid>()
            .ApplyTrack<Message>()
            .ApplyAudit<Message, Guid>();
    }
}
