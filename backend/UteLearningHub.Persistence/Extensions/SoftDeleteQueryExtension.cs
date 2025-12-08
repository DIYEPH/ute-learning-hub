using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Persistence.Extensions;

public static class SoftDeleteQueryExtension
{
    public static void ApplySoftDelteQueryFilters(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "x");
            var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var compare = Expression.Equal(property, Expression.Constant(false));
            var lamda = Expression.Lambda(compare, parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(lamda);
        }
    }
}
