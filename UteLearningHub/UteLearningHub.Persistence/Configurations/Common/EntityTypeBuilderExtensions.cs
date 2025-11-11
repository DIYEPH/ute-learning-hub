using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Persistence.Configurations.Common;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ApplyAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable
    {
        builder.Property(u => u.CreatedById).HasColumnName("TaoBoi");
        builder.Property(u => u.UpdatedById).HasColumnName("CapNhatBoi");
        return builder;
    }
    public static EntityTypeBuilder<TEntity> ApplyReview<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IReviewable
    {
        builder.Property(u => u.ReviewedById).HasColumnName("DuyetBoi");
        builder.Property(u => u.ReviewNote).HasColumnName("NoiDungDuyet");
        builder.Property(u => u.ReviewedAt).HasColumnName("NgayDuyet");
        builder.Property(u => u.ReviewStatus).HasColumnName("TrangThaiDuyet");
        return builder;
    }
    public static EntityTypeBuilder<TEntity> ApplySoftDelete<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDelete
    {
        builder.Property(u => u.IsDeleted).HasColumnName("CoDaXoa");
        builder.Property(u => u.DeletedAt).HasColumnName("NgayXoa");
        builder.Property(u => u.DeletedById).HasColumnName("BiXoaBoi");
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
