using MauiStudyApp.Domain.Data;

namespace MauiStudyApp.Domain.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Customer>> GetCustomersAsync();
    }
}