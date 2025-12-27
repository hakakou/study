using System.Diagnostics;
using Clean.Core.ContributorAggregate;
using Clean.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Test.Domain.AggregateModel;
using Xunit.Abstractions;

namespace Clean.IntegrationTests.Data;

public abstract class BaseEfRepoTestFixture : IDisposable
{
  protected AppDbContext _dbContext;
  private readonly SqliteConnection _connection;

  public ITestOutputHelper Output { get; }

  protected BaseEfRepoTestFixture(ITestOutputHelper output)
  {
    _connection = new SqliteConnection("DataSource=:memory:");
    _connection.Open();

    var options = CreateNewContextOptions();
    _dbContext = new AppDbContext(options);
    _dbContext.Database.EnsureCreated();
    Output = output;
  }

  protected DbContextOptions<AppDbContext> CreateNewContextOptions()
  {
    var fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();

    // Create a fresh service provider
    var serviceProvider = new ServiceCollection()
        .AddEntityFrameworkSqlite()
        .AddScoped<IDomainEventDispatcher>(_ => fakeEventDispatcher)
        .AddScoped<EventDispatchInterceptor>()
        .BuildServiceProvider();

    // Create a new options instance telling the context to use SQLite
    // and the new service provider.
    var interceptor = serviceProvider.GetRequiredService<EventDispatchInterceptor>();

    var builder = new DbContextOptionsBuilder<AppDbContext>();
    builder.UseInternalServiceProvider(serviceProvider)
           .UseSqlite(_connection) // Use the open connection
           .LogTo((c) => Trace.WriteLine(c), LogLevel.Information)
           .AddInterceptors(interceptor);

    return builder.Options;
  }

  protected EfRepository<Contributor> GetRepository()
  {
    return new EfRepository<Contributor>(_dbContext);
  }

  protected EfRepository<Issue> GetIssueRepository()
  {
    return new EfRepository<Issue>(_dbContext);
  }

  public void Dispose()
  {
    _dbContext?.Dispose();
    _connection?.Close();
    _connection?.Dispose();
  }
}
