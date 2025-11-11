using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Persistence.Configurations.Common;
using DomainType = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Persistence.Configurations;
public class TypeConfiguration : IEntityTypeConfiguration<DomainType>
{
    public void Configure(EntityTypeBuilder<DomainType> builder)
    {
        builder.ToTable(DbTableNames.Type);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.TypeName).HasColumnName("TenLoai");

        builder.ApplySoftDelete<DomainType>()
            .ApplyTrack<DomainType>()
            .ApplyAudit<DomainType>()
            .ApplyReview<DomainType>();
    }
}
