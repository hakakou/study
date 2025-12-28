using Ardalis.Specification;
using System.Linq.Expressions;

namespace Haka.Patterns.DDD;

/// <summary>
/// An abstraction for persistence, based on Ardalis.Specification
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}
