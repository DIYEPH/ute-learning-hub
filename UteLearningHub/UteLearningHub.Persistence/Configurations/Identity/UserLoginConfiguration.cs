using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetUserLogins);

        builder.Property(u => u.LoginProvider).HasColumnName("NhaCungCapDangNhap");
        builder.Property(u => u.ProviderKey).HasColumnName("KhoaNhaCungCap");
        builder.Property(u => u.ProviderDisplayName).HasColumnName("TenHienThiNhaCungCap");
        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
    }
}
