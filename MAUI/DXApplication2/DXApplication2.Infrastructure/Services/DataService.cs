using DXApplication2.Domain.Data;
using DXApplication2.Domain.Services;

namespace DXApplication2.Infrastructure.Services
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