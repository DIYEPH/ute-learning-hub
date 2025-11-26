using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;

namespace UteLearningHub.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable(DbTableNames.Event);
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasColumnName("TieuDe").HasMaxLength(200);
        builder.Property(e => e.ShortDescription).HasColumnName("MoTaNgan").HasMaxLength(500);
        builder.Property(e => e.Content).HasColumnName("NoiDung");
        builder.Property(e => e.ImageUrl).HasColumnName("AnhBanner").HasMaxLength(500);
        builder.Property(e => e.RedirectUrl).HasColumnName("DuongDan").HasMaxLength(500);
        builder.Property(e => e.StartAt).HasColumnName("BatDauLuc");
        builder.Property(e => e.EndAt).HasColumnName("KetThucLuc");
        builder.Property(e => e.IsActive).HasColumnName("DangHoatDong");
        builder.Property(e => e.Priority).HasColumnName("DoUuTien");

        builder.ApplySoftDelete<Event>()
               .ApplyTrack<Event>();
    }
}