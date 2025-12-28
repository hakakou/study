using Ardalis.Specification.EntityFrameworkCore;
using Haka.Patterns.DDD;
using Test.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.Infrastructure.Repositories;

public class EfRepository<T> : RepositoryBase<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public EfRepository(TestDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<T>> WhereAsync(Haka.Patterns.Specifications.ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.ApplySpecification(specification);
        var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list;
    }
}
