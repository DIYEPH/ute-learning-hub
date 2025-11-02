using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace Persistence.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<DomainFile>
{
    public void Configure(EntityTypeBuilder<DomainFile> builder)
    {
        builder.ToTable(DbTableNames.File);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.FileName).HasColumnName("TenTep");
        builder.Property(u => u.FileSize).HasColumnName("KichThuoc");
        builder.Property(u => u.FileType).HasColumnName("LoaiFile");
        builder.Property(u => u.FileUrl).HasColumnName("LinkTruyCap");

        builder.ApplySoftDelete<DomainFile, Guid>()
            .ApplyTrack<DomainFile>()
            .ApplyAudit<DomainFile, Guid>();
    }
}
