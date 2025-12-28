using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SpecificationPattern.Database.Entities;

namespace SpecificationPattern.Database;

public class ApplicationDbContext(
	DbContextOptions<ApplicationDbContext> options)
	: DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Like> Likes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
