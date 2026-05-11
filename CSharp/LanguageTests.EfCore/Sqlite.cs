using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace LanguageTests.Packages
{
    public class SQLite
    {
        public ITestOutputHelper OutputHelper { get; }

        public SQLite(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }


        [Fact]
        private void TestDb()
        {
            var db = new Northwind();
            var loggerFactory = db.GetService<ILoggerFactory>();
            //loggerFactory.AddProvider(new XUnitLoggerProvider(OutputHelper));

            db.ChangeTracker.LazyLoadingEnabled = false;

            var cats = db.Categories;
            foreach (var c in cats)
            {
                var products = db.Entry(c).Collection(q => q.Products);
                if (!products.IsLoaded)
                    products.Load();
                OutputHelper.WriteLine($"{c.CategoryName} {c.Products.Count()}");
            }
        }
    }

    public class Category
    {
        // these properties map to columns in the database
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        // defines a navigation property for related rows
        public virtual ICollection<Product> Products { get; set; }

        public Category()
        {
            // to enable developers to add products to a Category we must
            // initialize the navigation property to an empty list
            this.Products = new List<Product>();
        }
    }

    public class Product
    {
        public int ProductID { get; set; }

        [Required]
        [StringLength(40)]
        public string ProductName { get; set; }

        [Column("UnitPrice", TypeName = "money")]
        public decimal? Cost { get; set; }

        [Column("UnitsInStock")]
        public short? Stock { get; set; }

        public bool Discontinued { get; set; }

        // these two define the foreign key relationship
        // to the Categories table
        public int CategoryID { get; set; }

        public virtual Category Category { get; set; }
    }

    // this manages the connection to the database
    public class Northwind : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = @"c:\Files\Projects\Northwind\Northwind.db";
            optionsBuilder.UseLazyLoadingProxies()
                .UseSqlite($"Filename={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // example of using Fluent API instead of attributes
            // to limit the length of a category name to 15
            modelBuilder.Entity<Category>()
                .Property(category => category.CategoryName)
                .IsRequired() // NOT NULL
                .HasMaxLength(15);

            modelBuilder.Entity<Product>()
                .HasQueryFilter(q => !q.Discontinued);
        }
    }
}
