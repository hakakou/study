using Haka.Patterns.DDD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Test.Infrastructure.Data;
using Test.Infrastructure.Repositories;
using Xunit;

namespace Test.Infrastructure.Tests;

public class DatabaseFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public SqlCommandInterceptor SqlInterceptor { get; }

    public DatabaseFixture()
    {
        var services = new ServiceCollection();
        SqlInterceptor = new SqlCommandInterceptor();

        services.AddScoped<TestDbContext>(provider =>
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .LogTo(c=> Trace.WriteLine(c))
                .AddInterceptors(SqlInterceptor)
                .Options;

            var context = new TestDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        });

        // Register repository pattern
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        //services.AddScoped(typeof(IUnitOfWork<>), typeof(EfUnitOfWork<>));

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}

[CollectionDefinition("DI")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
