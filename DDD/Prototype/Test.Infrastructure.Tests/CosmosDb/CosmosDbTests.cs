using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Test.Infrastructure.Tests.CosmosDb;

public class CosmosDbTests
{
    private const string DatabaseName = "TestDb";
    private readonly string _connectionString;

    public CosmosDbTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<CosmosDbTests>()
            .Build();

        _connectionString = configuration["CosmosDb"] ?? throw new InvalidOperationException("CosmosDb connection string not found in user secrets.");
    }

    private CosmosTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CosmosTestDbContext>()
            .UseCosmos(
                connectionString: _connectionString,
                databaseName: DatabaseName)
            .Options;

        return new CosmosTestDbContext(options);
    }

    [Fact]
    public async Task Should_Add_And_Retrieve_Product()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product",
            Category = "Electronics",
            Price = 99.99m,
            Stock = 50,
            CreatedDate = DateTime.UtcNow
        };

        // Act - Add product
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Assert - Retrieve product
        var retrievedProduct = await context.Products
            .WithPartitionKey(product.Id)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.NotNull(retrievedProduct);
        Assert.Equal(product.Name, retrievedProduct.Name);
        Assert.Equal(product.Category, retrievedProduct.Category);
        Assert.Equal(product.Price, retrievedProduct.Price);
        Assert.Equal(product.Stock, retrievedProduct.Stock);
    }

    [Fact]
    public async Task Should_Update_Product()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Original Product",
            Category = "Books",
            Price = 19.99m,
            Stock = 100
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act - Update product
        product.Name = "Updated Product";
        product.Price = 24.99m;
        product.Stock = 75;
        await context.SaveChangesAsync();

        // Assert - Retrieve updated product
        var updatedProduct = await context.Products
            .WithPartitionKey(product.Id)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.NotNull(updatedProduct);
        Assert.Equal("Updated Product", updatedProduct.Name);
        Assert.Equal(24.99m, updatedProduct.Price);
        Assert.Equal(75, updatedProduct.Stock);
    }

    [Fact]
    public async Task Should_Delete_Product()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Product to Delete",
            Category = "Toys",
            Price = 15.99m,
            Stock = 30
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act - Delete product
        context.Products.Remove(product);
        await context.SaveChangesAsync();

        // Assert - Try to retrieve deleted product
        var deletedProduct = await context.Products
            .WithPartitionKey(product.Id).SingleOrDefaultAsync();

        //var deletedProduct = await context.Products
        //    .SingleOrDefaultAsync(c=> c.Id == product.Id);

        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task Should_Query_Products_By_Category()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var categoryId = Guid.NewGuid().ToString();
        var products = new[]
        {
            // new Product { Id = categoryId, Name = "Product 1", Category = "Electronics", Price = 99.99m, Stock = 10 },
            new Product { Id = categoryId, Name = "Product 2", Category = "Electronics", Price = 149.99m, Stock = 5 },
            new Product { Id = Guid.NewGuid().ToString(), Name = "Product 3", Category = "Books", Price = 19.99m, Stock = 50 }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Act - Query by category using partition key
        var electronicsProducts = await context.Products
             .WithPartitionKey(categoryId)
            .Where(p => p.Category == "Electronics")
            .ToListAsync();

        // Assert
        Assert.NotEmpty(electronicsProducts);
        Assert.All(electronicsProducts, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public async Task Should_Demonstrate_Cosmos_Specific_Features()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var productId = Guid.NewGuid().ToString();
        var product = new Product
        {
            Id = productId,
            Name = "Cosmos Demo Product",
            Category = "Demo",
            Price = 999.99m,
            Stock = 1
        };

        // Act & Assert - Demonstrate various Cosmos DB operations

        // 1. Add with explicit partition key
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // 2. Query with partition key (more efficient)
        var queriedProduct = await context.Products
            .WithPartitionKey(productId)
            .FirstOrDefaultAsync(p => p.Id == productId);
        Assert.NotNull(queriedProduct);

        // 3. Cross-partition query (less efficient, for demonstration)
        var allProducts = await context.Products
            .Where(p => p.Price > 100)
            .ToListAsync();
        Assert.NotEmpty(allProducts);

        // 4. Count operations
        var productCount = await context.Products
            .WithPartitionKey(productId)
            .CountAsync();
        Assert.True(productCount >= 1);
    }

    [Fact]
    public async Task Should_Handle_Bulk_Operations()
    {
        // Arrange
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var products = Enumerable.Range(1, 5).Select(i => new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"Bulk Product {i}",
            Category = "Bulk",
            Price = 10.00m * i,
            Stock = i * 10
        }).ToList();

        // Act - Bulk insert
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Assert - Verify products were created
        foreach (var product in products)
        {
            var retrievedProduct = await context.Products
                .WithPartitionKey(product.Id)
                .FirstOrDefaultAsync(p => p.Id == product.Id);
            Assert.NotNull(retrievedProduct);
        }
    }
}
