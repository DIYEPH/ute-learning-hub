using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class DocumentAuthorConfiguration : IEntityTypeConfiguration<DocumentAuthor>
{
    public void Configure(EntityTypeBuilder<DocumentAuthor> builder)
    {
        builder.ToTable(DbTableNames.DocumentAuthor);

        builder.HasKey(u => new { u.DocumentId, u.AuthorId });

        builder.Property(u => u.DocumentId).HasColumnName("TaiLieuId");
        builder.Property(u => u.AuthorId).HasColumnName("TacGiaId");
        builder.Property(u => u.Role).HasColumnName("VaiTro");

        builder.HasOne(u => u.Document)
            .WithMany(u => u.DocumentAuthors)
            .HasForeignKey(u => u.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Author)
            .WithMany(u => u.DocumentAuthors)
            .HasForeignKey(u => u.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


