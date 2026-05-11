using Microsoft.EntityFrameworkCore;

namespace CoffeeChainApp;

public class CoffeeChainDbContext : DbContext
{
    private readonly string _connectionString;

    public CoffeeChainDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(_connectionString);
}
