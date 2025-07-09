
namespace MauiStudyApp.Domain.Repositories
{
    public interface IGenericRepository<TEntity>
    {
        Task<IEnumerable<TEntity>?> GetAsync();
        Task<TEntity?> GetByIdAsync(int id);
        void Add(TEntity item);
        void Delete(TEntity item);
        void Update(TEntity entityToUpdate);
    }
}