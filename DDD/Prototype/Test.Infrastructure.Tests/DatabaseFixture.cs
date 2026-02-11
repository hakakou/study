using System;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Test.Application.EventHandlers;
using Test.Domain.Events;
using Test.Infrastructure.Data;
using Test.Infrastructure.Repositories;
using Xunit;
using Haka.Patterns.SeedWork;

namespace Test.Infrastructure.Tests;

public class DatabaseFixture : IDisposable
{
    public ServiceProvider ServiceProvider { get; }
    public SqlCommandInterceptor SqlInterceptor { get; }

    public DatabaseFixture()
    {
        var services = new ServiceCollection();
        SqlInterceptor = new SqlCommandInterceptor();

        services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.Assemblies = [
                    typeof(IssueAssignedToUserDomainEventHandler),
                    typeof(IssueAssignedToUserDomainEvent)];
            }
        );

        services.AddScoped<EventDispatchInterceptor>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
        services.AddLogging((b) => b.AddXUnit());

        services.AddScoped<TestDbContext>(provider =>
        {
            var eventDispatchInterceptor = provider.GetRequiredService<EventDispatchInterceptor>();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .LogTo(c => Trace.WriteLine(c))
                .AddInterceptors(SqlInterceptor, eventDispatchInterceptor)
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
