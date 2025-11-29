using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;
public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable(DbTableNames.Subject);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.SubjectName).HasColumnName("TenMonHoc");
        builder.Property(u => u.SubjectCode).HasColumnName("MaMonHoc");

        builder.ApplySoftDelete<Subject>()
            .ApplyTrack<Subject>()
            .ApplyAudit<Subject>()
            .ApplyReview<Subject>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Subjects)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
