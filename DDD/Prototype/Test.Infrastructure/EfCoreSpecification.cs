using Haka.Patterns.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Test.Infrastructure;

public static class EfCoreSpecification
{
    public static IQueryable<TEntity> Apply<TEntity>(this Specification<TEntity> specification,
        IQueryable<TEntity> queryable) where TEntity : class
    {
        if (specification.FilterQuery is not null)
        {
            queryable = queryable.Where(specification.FilterQuery);
        }

        if (specification.IncludeQueries?.Count > 0)
        {
            queryable = specification.IncludeQueries.Aggregate(queryable,
                (current, includeQuery) => current.Include(includeQuery));
        }

        if (specification.OrderByQueries?.Count > 0)
        {
            var orderedQueryable = queryable.OrderBy(specification.OrderByQueries.First());

            orderedQueryable = specification.OrderByQueries.Skip(1)
                .Aggregate(orderedQueryable, (current, orderQuery) => current.ThenBy(orderQuery));

            queryable = orderedQueryable;
        }

        if (specification.OrderByDescendingQueries?.Count > 0)
        {
            var orderedQueryable = queryable.OrderByDescending(specification.OrderByDescendingQueries.First());

            orderedQueryable = specification.OrderByDescendingQueries.Skip(1)
                .Aggregate(orderedQueryable, (current, orderQuery) => current.ThenByDescending(orderQuery));

            queryable = orderedQueryable;
        }

        return queryable;
    }
}
