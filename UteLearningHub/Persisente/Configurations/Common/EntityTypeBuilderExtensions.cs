using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Entities.Base;

namespace Persistence.Configurations.Common;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ApplyAudit<TEntity, TKey>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable<TKey>
    {
        builder.Property(u => u.CreatedBy).HasColumnName("TaoBoi");
        builder.Property(u => u.UpdatedBy).HasColumnName("CapNhatBoi");
        return builder;
    }
    public static EntityTypeBuilder<TEntity> ApplyReview<TEntity, TKey>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IReviewable<TKey>
    {
        builder.Property(u => u.ReviewedBy).HasColumnName("DuyetBoi");
        builder.Property(u => u.ReviewNote).HasColumnName("NoiDungDuyet");
        builder.Property(u => u.ReviewedAt).HasColumnName("NgayDuyet");
        builder.Property(u => u.ReviewStatus).HasColumnName("TrangThaiDuyet");
        return builder;
    }
    public static EntityTypeBuilder<TEntity> ApplySoftDelete<TEntity, TKey>(this EntityTypeBuilder<TEntity> builder)
        where TEntity: class, ISoftDelete<TKey>
    {
        builder.Property(u => u.IsDeleted).HasColumnName("CoDaXoa");
        builder.Property(u => u.DeletedAt).HasColumnName("NgayXoa");
        builder.Property(u => u.DeletedBy).HasColumnName("BiXoaBoi");
        return builder;
    }
    public static EntityTypeBuilder<TEntity> ApplyTrack<TEntity>(this EntityTypeBuilder<TEntity> builder)
    where TEntity : class, ITrackable
    {
        builder.Property(u => u.RowVersion).IsRowVersion().HasColumnName("PhienBanHang");
        builder.Property(u => u.CreatedAt).HasColumnName("NgayTao");
        builder.Property(u => u.UpdatedAt).HasColumnName("NgayCapNhat");
        return builder;
    }
}
