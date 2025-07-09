using MauiStudyApp.Domain.Data;
using MauiStudyApp.Domain.Services;

namespace MauiStudyApp.Infrastructure.Services
{
    public class DataService : IDataService
    {
        public async Task<IEnumerable<Customer>> GetCustomersAsync()
        {
            await Task.Delay(2500);
            return Enumerable.Range(1, 8).Select(x => new Customer(x)).ToList();
        }
    }
}