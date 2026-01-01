using Microsoft.EntityFrameworkCore;

namespace WebAPI2;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.LogTo((e, l) => true, e =>
        //{
        //    if (_logger.IsEnabled(e.LogLevel))
        //        _logger.Log(e.LogLevel, e.EventId, e.ToString());
        //}).EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop for developers",
                Price = 1299.99m
            },
            new Product
            {
                Id = 2,
                Name = "Mouse",
                Description = "Ergonomic wireless mouse",
                Price = 29.99m
            },
            new Product
            {
                Id = 3,
                Name = "Keyboard",
                Description = "Mechanical keyboard with RGB lighting",
                Price = 149.99m
            });
    }
}

public static class AppDbContextExtensions
{
    public static async Task EnsureDbSeeded(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await context.Database.EnsureCreatedAsync();
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
