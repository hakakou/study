using Ardalis.Specification.EntityFrameworkCore;
using Haka.Patterns.DDD;
using Test.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.Infrastructure.Repositories;

public class EfRepository<T>(TestDbContext dbContext)
    : RepositoryBase<T>(dbContext), IRepository<T> where T : class, IAggregateRoot
{

    public async Task<List<T>> WhereAsync(Haka.Patterns.Specifications.ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.ApplySpecification(specification);
        var list = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return list;
    }

    //public IUnitOfWork<T> UnitOfWork => new EfUnitOfWork<T>(dbContext);
}

/*
public class EfUnitOfWork<T>(TestDbContext context) : IUnitOfWork<T>, IDisposable
    where T : class, IAggregateRoot
{
    readonly TestDbContext Context = context;

    IRepository<T>? customerRepository;

    // public IRepository<T> Items => customerRepository ??= new EfRepository<T>(Context);

    public Task<int> SaveAsync()
    {
        return Context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
            Context.Dispose();

        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~EfUnitOfWork()
    {
        Dispose(false);
    }

    bool disposed = false;
}
*/