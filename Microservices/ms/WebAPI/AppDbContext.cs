using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace WebAPI;

internal class PlanetDbContext : DbContext
{
    public DbSet<Planet> Planets { get; init; }

    private ILogger<PlanetDbContext> _logger;

    public PlanetDbContext(DbContextOptions options, ILogger<PlanetDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo((e, l) => true, e =>
        {
            if (_logger.IsEnabled(e.LogLevel))
                _logger.Log(e.LogLevel, e.EventId, e.ToString());
        }).EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        _logger.LogInformation("Hello");
        base.OnModelCreating(b);
        b.Entity<Planet>().ToCollection("planets");

        b.Entity<Planet>().HasData(
             new Planet
             {
                 Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                 Name = "Gaga",
                 Description = "Second planet from the Sun and has a thick atmosphere."
             });
    }
}

public static class PlanetDbContextExtensions
{
    public static async Task EnsureDbSeeded(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PlanetDbContext>();

        if (context.Database.EnsureCreated())
        {
            context.Planets.Add(new Planet
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Venus",
                Description = "Second planet from the Sun and has a thick atmosphere."
            });
            await context.SaveChangesAsync();

            context.Planets.Add(new Planet
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Mars",
                Description = "Fourth planet from the Sun and known as the Red Planet."
            });
            await context.SaveChangesAsync();
        }
    }
}

public class Planet
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}