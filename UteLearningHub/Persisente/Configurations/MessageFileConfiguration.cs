using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace Persistence.Configurations;

public class MessageFileConfiguration : IEntityTypeConfiguration<MessageFile>
{
    public void Configure(EntityTypeBuilder<MessageFile> builder)
    {
        builder.ToTable(DbTableNames.MessageFile);

        builder.HasKey(u => new { u.FileId, u.MessageId });

        builder.Property(u => u.FileId).HasColumnName("TepId");
        builder.Property(u => u.MessageId).HasColumnName("TinNhanId");
    }
}
