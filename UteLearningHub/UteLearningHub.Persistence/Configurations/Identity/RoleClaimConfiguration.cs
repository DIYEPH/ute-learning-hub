using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetRoleClaims);

        builder.Property(u => u.RoleId).HasColumnName("VaiTroId");
        builder.Property(u => u.ClaimType).HasColumnName("LoaiQuyen");
        builder.Property(u => u.ClaimValue).HasColumnName("GiaTriQuyen");
    }
}
