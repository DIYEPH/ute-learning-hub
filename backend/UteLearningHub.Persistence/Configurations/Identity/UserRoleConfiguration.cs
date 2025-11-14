using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;

namespace UteLearningHub.Persistence.Configurations.Identity;

public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable(DbTableNames.AspNetUserRoles);

        builder.Property(u => u.UserId).HasColumnName("NguoiDungId");
        builder.Property(u => u.RoleId).HasColumnName("VaiTroId");
    }
}
