using Microsoft.EntityFrameworkCore;

namespace Test.Infrastructure.Tests.CosmosDb;

public class CosmosTestDbContext : DbContext
{
    public CosmosTestDbContext(DbContextOptions<CosmosTestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToContainer("Product2");          
            // Configure the partition key
            entity.HasPartitionKey(p => p.Id);
            entity.HasKey(p => p.Id);
        });
    }
}
