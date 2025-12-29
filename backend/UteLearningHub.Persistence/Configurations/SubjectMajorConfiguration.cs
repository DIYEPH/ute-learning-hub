using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class SubjectMajorConfiguration : IEntityTypeConfiguration<SubjectMajor>
{
    public void Configure(EntityTypeBuilder<SubjectMajor> builder)
    {
        builder.ToTable(DbTableNames.SubjectMajor);

        builder.HasKey(sm => new { sm.SubjectId, sm.MajorId });

        builder.Property(sm => sm.SubjectId).HasColumnName("MonHocId").IsRequired();
        builder.Property(sm => sm.MajorId).HasColumnName("NganhId").IsRequired();

        builder.HasOne(sm => sm.Subject)
            .WithMany(s => s.SubjectMajors)
            .HasForeignKey(sm => sm.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.Major)
            .WithMany(m => m.SubjectMajors)
            .HasForeignKey(sm => sm.MajorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter: exclude when subject or major is deleted
        builder.HasQueryFilter(sm => !sm.Subject.IsDeleted && !sm.Major.IsDeleted);
    }
}
