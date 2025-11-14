using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetUserTokens);

        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
        builder.Property(u => u.LoginProvider).HasColumnName("NhaCungCapDangNhap");
        builder.Property(u => u.Name).HasColumnName("LoaiMa");
        builder.Property(u => u.Value).HasColumnName("GiaTriMa");
    }
}
