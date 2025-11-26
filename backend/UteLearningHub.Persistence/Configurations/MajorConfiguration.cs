using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class MajorConfiguration : IEntityTypeConfiguration<Major>
{
    public void Configure(EntityTypeBuilder<Major> builder)
    {
        builder.ToTable(DbTableNames.Major);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.MajorName).HasColumnName("TenNganh");
        builder.Property(u => u.MajorCode).HasColumnName("MaNganh");
        builder.Property(u => u.FacultyId).HasColumnName("KhoaId").IsRequired();

        builder.ApplySoftDelete<Major>()
            .ApplyTrack<Major>()
            .ApplyAudit<Major>();

        builder.HasOne(u => u.Faculty)
            .WithMany(u => u.Majors)
            .HasForeignKey(u => u.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
