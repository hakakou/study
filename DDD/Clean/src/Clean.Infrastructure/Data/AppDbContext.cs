using Clean.Core.ContributorAggregate;
using Clean.Infrastructure.Data.Config;
using Test.Domain.AggregateModel;

namespace Clean.Infrastructure.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Contributor> Contributors => Set<Contributor>();
  public DbSet<Issue> Issues => Set<Issue>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }

  public override int SaveChanges() =>
        SaveChangesAsync().GetAwaiter().GetResult();

  protected override void ConfigureConventions(
    ModelConfigurationBuilder configurationBuilder)
  {
    base.ConfigureConventions(configurationBuilder);

    configurationBuilder.RegisterAllInVogenEfCoreConverters();
  }
}
