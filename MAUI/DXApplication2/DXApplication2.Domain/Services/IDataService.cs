using DXApplication2.Domain.Data;

namespace DXApplication2.Domain.Services
{
    public interface IDataService
    {
        Task<IEnumerable<Customer>> GetCustomersAsync();
    }
}