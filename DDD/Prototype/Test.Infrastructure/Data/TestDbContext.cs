using Microsoft.EntityFrameworkCore;
using SmartEnum.EFCore;
using Test.Domain.AggregateModel.IssueAggregate;
using Test.Domain.AggregateModel.OrderAggregate;
using Test.Domain.AggregateModel.RepoAggregate;

namespace Test.Infrastructure.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Repo> Repos => Set<Repo>();   
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TestDbContext).Assembly);
        modelBuilder.ConfigureSmartEnum();
   }

}
