using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable(DbTableNames.AspNetUsers);

        builder.ApplySoftDelete<AppUser>()
            .ApplyTrack<AppUser>();

        builder.Property(u => u.MajorId).HasColumnName("MaNganh");
        builder.Property(u => u.Introduction).HasColumnName("GioiThieu");
        builder.Property(u => u.IsSuggest).HasColumnName("CoGoiY");
        builder.Property(u => u.Gender).HasColumnName("GioiTinh");
        builder.Property(u => u.AvatarUrl).HasColumnName("HinhDaiDien");
        builder.Property(u => u.TrustScore).HasColumnName("DiemXacThuc");
        builder.Property(u => u.TrustLever).HasColumnName("CapDoXacThuc");
        builder.Property(u => u.LastLoginAt).HasColumnName("LanDangNhapGanNhat");

        builder.Property(u => u.UserName).HasColumnName("TenDangNhap");
        builder.Property(u => u.FullName).HasColumnName("HoVaTen");
        builder.Property(u => u.Gender).HasColumnName("GioiTinh");
        builder.Property(u => u.NormalizedUserName).HasColumnName("TenDangNhapChuanHoa");
        builder.Property(u => u.NormalizedEmail).HasColumnName("EmailChuanHoa");
        builder.Property(u => u.EmailConfirmed).HasColumnName("CoXacThucEmail");
        builder.Property(u => u.PasswordHash).HasColumnName("MatKhauBam");
        builder.Property(u => u.SecurityStamp).HasColumnName("MaBaoMat");
        builder.Property(u => u.ConcurrencyStamp).HasColumnName("DauKiemSoatDongBo");
        builder.Property(u => u.PhoneNumber).HasColumnName("SoDienThoai");
        builder.Property(u => u.PhoneNumberConfirmed).HasColumnName("CoDaXacNhanSoDienThoai");
        builder.Property(u => u.TwoFactorEnabled).HasColumnName("CoKichHoatXacThucHaiLop");
        builder.Property(u => u.LockoutEnd).HasColumnName("NgayHetKhoaTaiKhoan");
        builder.Property(u => u.LockoutEnabled).HasColumnName("ChoPhepKhoaTaiKhoan");
        builder.Property(u => u.AccessFailedCount).HasColumnName("SoLanDangNhapThatBai");
    }
}
