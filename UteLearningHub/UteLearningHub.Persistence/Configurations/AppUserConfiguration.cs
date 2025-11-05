using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Persistence.Identity;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable(DbTableNames.AspNetUsers);

        builder.Property(u => u.MajorId).HasColumnName("MaNganh");
        builder.Property(u => u.Introduction).HasColumnName("GioiThieu");
        builder.Property(u => u.IsSuggest).HasColumnName("CoGoiY");
        builder.Property(u => u.Gender).HasColumnName("GioiTinh");

        builder.Property(u => u.UserName).HasColumnName("TenDangNhap").HasColumnType("varchar(100)");

        builder.Property(u => u.RowVersion).HasColumnName("PhienBanHang");
        builder.Property(u => u.CreatedAt).HasColumnName("NgayTao");
        builder.Property(u => u.UpdatedAt).HasColumnName("NgayCapNhat");
    }
}
