using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database;
using SpecificationPattern.Specifications;
using SpecificationPattern.Specifications.Base;

namespace SpecificationPattern.Repositories;

public static class SpecificationExtensions
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this ApplicationDbContext dbContext,
        ISpecification<TEntity> specification) where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(specification, nameof(specification));

        var efCoreSpecification = new EfCoreSpecification<TEntity>(specification);

        var query = dbContext.Set<TEntity>().AsNoTracking();
        query = efCoreSpecification.Apply(query);

        return query;
    }

    public static async Task<List<TEntity>> WhereAsync<TEntity>(
	    this ApplicationDbContext dbContext,
	    ISpecification<TEntity> specification,
	    CancellationToken cancellationToken = default) where TEntity : class
    {
	    ArgumentNullException.ThrowIfNull(specification, nameof(specification));
	    cancellationToken.ThrowIfCancellationRequested();

	    var efCoreSpecification = new EfCoreSpecification<TEntity>(specification);

	    var query = dbContext.Set<TEntity>().AsNoTracking();
	    query = efCoreSpecification.Apply(query);

	    return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
