using Ardalis.Specification.EntityFrameworkCore;
using Haka.Patterns.DDD;
using System.Linq.Expressions;
using Test.Infrastructure.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Test.Infrastructure.Repositories;

public class EfRepository<T> : RepositoryBase<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    public EfRepository(TestDbContext dbContext) : base(dbContext)
    {
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return DbContext.Set<T>().FirstOrDefaultAsync(predicate);
    }
}
