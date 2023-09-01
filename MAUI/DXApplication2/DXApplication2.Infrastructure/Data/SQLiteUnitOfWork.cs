using DXApplication2.Domain.Data;
using DXApplication2.Domain.Services;
using DXApplication2.Infrastructure.Repositories;

namespace DXApplication2.Infrastructure.Data
{
    public class SQLiteUnitOfWork : IDisposable
    {
        readonly EntitiesContext context;
        readonly ICacheService cacheService;

        GenericRepository<Customer>? customersRepository;
        public GenericRepository<Customer> CustomersRepository
        {
            get => customersRepository ??= new GenericRepository<Customer>(context, cacheService);
        }

        public SQLiteUnitOfWork(ICacheService cacheService)
        {
            this.cacheService = cacheService;
            this.context = new EntitiesContext();
        }

        public Task SaveAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    context.SaveChanges();
                    CustomersRepository.ExecuteCacheUpdateActions();
                }
                catch
                {
                    CustomersRepository.ClearCacheUpdateActions();
                    throw;
                }
            });
        }
        public void Dispose()
        {
            context.Dispose();
        }
    }
}