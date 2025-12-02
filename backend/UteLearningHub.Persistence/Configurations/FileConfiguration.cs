using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Persistence.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<DomainFile>
{
    public void Configure(EntityTypeBuilder<DomainFile> builder)
    {
        builder.ToTable(DbTableNames.File);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.FileName).HasColumnName("TenTep");
        builder.Property(u => u.FileSize).HasColumnName("KichThuoc");
        builder.Property(u => u.MimeType).HasColumnName("LoaiFile");
        builder.Property(u => u.FileUrl).HasColumnName("LinkTruyCap");
        builder.Property(u => u.IsTemporary).HasColumnName("LaTam").HasDefaultValue(false);

        builder.ApplySoftDelete<DomainFile>()
            .ApplyTrack<DomainFile>()
            .ApplyAudit<DomainFile>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Files)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
