using Ardalis.Specification;
using business_logic.DTOs;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public static class SpecificationBuilderPagingExtensions
    {
        public static void OrderBySortDirection<TEntity>(
            this ISpecificationBuilder<TEntity> query,
            Expression<Func<TEntity, object?>> sortKeySelector,
            Expression<Func<TEntity, object?>> tieBreakerKeySelector,
            SortDirection sortDirection)
        {
            var isAscendingSortDirection = sortDirection == SortDirection.Ascending;
            query.OrderBy(sortKeySelector, isAscendingSortDirection).ThenBy(tieBreakerKeySelector);
            query.OrderByDescending(sortKeySelector, !isAscendingSortDirection).ThenBy(tieBreakerKeySelector);
        }

        public static void ApplyPage<TEntity>(this ISpecificationBuilder<TEntity> query, int page, int pageSize)
        {
            query.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
