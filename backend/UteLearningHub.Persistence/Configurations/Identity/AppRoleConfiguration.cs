using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class AppRoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetRoles);

        builder.Property(u => u.Name).HasColumnName("TenVaiTro");
        builder.Property(u => u.NormalizedName).HasColumnName("TenChuanHoa");
        builder.Property(u => u.ConcurrencyStamp).HasColumnName("DauKiemSoatDongBo");
    }
}
