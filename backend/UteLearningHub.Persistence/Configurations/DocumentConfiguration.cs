using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(DbTableNames.Document);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.SubjectId).HasColumnName("MonHocId");
        builder.Property(u => u.TypeId).HasColumnName("LoaiTaiLieuId");
        builder.Property(u => u.Description).HasColumnName("MoTa");
        builder.Property(u => u.NormalizedName).HasColumnName("TenChuanHoa");
        builder.Property(u => u.IsDownload).HasColumnName("CoDuocTai");
        builder.Property(u => u.Visibility).HasColumnName("CoHienThi");
        builder.Property(u => u.CoverFileId).HasColumnName("TepBiaId");

        builder.ApplySoftDelete<Document>()
            .ApplyTrack<Document>()
            .ApplyAudit<Document>()
            .ApplyReview<Document>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Documents)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Subject)
            .WithMany(u => u.Documents)
            .HasForeignKey(u => u.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Type)
            .WithMany(u => u.Documents)
            .HasForeignKey(u => u.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.CoverFile)
            .WithMany()
            .HasForeignKey(u => u.CoverFileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
