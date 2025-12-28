using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Infrastructure.Data;
using Test.Infrastructure.Repositories;
using Haka.Patterns.DDD;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;

namespace Test.Infrastructure.Tests;

public class DatabaseFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }

    public DatabaseFixture()
    {
        var services = new ServiceCollection();

        services.AddScoped<TestDbContext>(provider =>
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .LogTo(c=> Trace.WriteLine(c))
                .Options;

            var context = new TestDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        });

        // Register repository pattern
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}

[CollectionDefinition("DI")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
