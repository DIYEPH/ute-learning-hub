using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(DbTableNames.Document);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.SubjectId).HasColumnName("MonHocId");
        builder.Property(u => u.TypeId).HasColumnType("TheId");
        builder.Property(u => u.Description).HasColumnType("MoTa");
        builder.Property(u => u.AuthorName).HasColumnName("TacGia");
        builder.Property(u => u.DescriptionAuthor).HasColumnName("MoTaTacGia");
        builder.Property(u => u.Slug).HasColumnName("TenThanThien");
        builder.Property(u => u.IsDownload).HasColumnName("CoDuocTai");
        builder.Property(u => u.Visibility).HasColumnName("CoHienThi");

        builder.ApplySoftDelete<Document, Guid>()
            .ApplyTrack<Document>()
            .ApplyAudit<Document, Guid>()
            .ApplyReview<Document, Guid>();
    }
}
