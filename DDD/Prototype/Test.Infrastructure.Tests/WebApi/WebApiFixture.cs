using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Infrastructure.Data;
using Xunit;

namespace Test.Infrastructure.Tests.WebApi;

public class WebApiFixture : WebApplicationFactory<Program>
{
    // Keep connection open so the in-memory database persists across requests
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registrations
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<TestDbContext>)
                         || d.ServiceType == typeof(TestDbContext))
                .ToList();
            foreach (var d in descriptors)
                services.Remove(d);

            // Replace with in-memory SQLite sharing the open connection
            services.AddDbContext<TestDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(sp.GetRequiredService<EventDispatchInterceptor>());
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

[CollectionDefinition("WebApi")]
public class WebApiCollection : ICollectionFixture<WebApiFixture>;
