using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetUserClaims);

        builder.Property(u => u.ClaimValue).HasColumnName("GiaTriQuyen");
        builder.Property(u => u.ClaimType).HasColumnName("LoaiQuyen");
        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
    }
}
