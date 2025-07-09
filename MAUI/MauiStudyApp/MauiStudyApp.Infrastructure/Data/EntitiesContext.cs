using MauiStudyApp.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace MauiStudyApp.Infrastructure.Data
{
    public class EntitiesContext : DbContext
    {
        public DbSet<Customer>? Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            Seed(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "data.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
            base.OnConfiguring(optionsBuilder);
        }

        private void Seed(ModelBuilder modelBuilder)
        {
            var customers = new List<Customer>();
            for (int i = 1; i <= 10; i++)
                customers.Add(new Customer(i));
            modelBuilder.Entity<Customer>().HasData(customers);
        }
    }
}