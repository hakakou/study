using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI;

internal class PlanetDbContext : DbContext
{
    public DbSet<Planet> Planets { get; init; }

    public static PlanetDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<PlanetDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);

    public PlanetDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Planet>().ToCollection("planets");

        b.Entity<Planet>().HasData(
             new Planet
             {
                 Id = Guid.NewGuid(),
                 Name = "Venus",
                 Description = "Second planet from the Sun and has a thick atmosphere."
             });
    }
}

public class Planet
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}