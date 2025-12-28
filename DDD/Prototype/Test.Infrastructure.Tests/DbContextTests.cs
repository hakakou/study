using Microsoft.EntityFrameworkCore;
using Test.Domain.AggregateModel;
using Test.Infrastructure.Data;
using Xunit;

namespace Test.Infrastructure.Tests;

public class DbContextTests : IDisposable
{
    private readonly TestDbContext _context;

    public DbContextTests()
    {
        // Create an in-memory SQLite database
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new TestDbContext(options);
        
        // Open the connection and create the database schema
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CanInsertGitRepository()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new GitRepository(repositoryId, "TestRepository") { Name = "TestRepository" };

        // Act
        _context.GitRepositories.Add(repository);
        await _context.SaveChangesAsync();

        // Assert
        var savedRepository = await _context.GitRepositories
            .FirstOrDefaultAsync(r => r.Id == repositoryId);
        
        Assert.NotNull(savedRepository);
        Assert.Equal("TestRepository", savedRepository.Name);
        Assert.Equal(repositoryId, savedRepository.Id);
    }

    [Fact]
    public async Task CanInsertIssue()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new GitRepository(repositoryId, "TestRepository") { Name = "TestRepository" };
        _context.GitRepositories.Add(repository);
        await _context.SaveChangesAsync();

        var createdDate = DateTime.UtcNow;
        var issue = new Issue(repositoryId, "Test Issue", createdDate);
        issue.Description = "This is a test issue";

        // Act
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        // Assert
        var savedIssue = await _context.Issues
            .FirstOrDefaultAsync(i => i.Name == "Test Issue");
        
        Assert.NotNull(savedIssue);
        Assert.Equal("Test Issue", savedIssue.Name);
        Assert.Equal("This is a test issue", savedIssue.Description);
        Assert.Equal(repositoryId, savedIssue.GitRepositoryId);
        Assert.Equal(createdDate, savedIssue.CreatedDate);
    }

    [Fact]
    public async Task CanInsertMultipleEntities()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new GitRepository(repositoryId, "MainRepository") { Name = "MainRepository" };
        
        var issue1 = new Issue(repositoryId, "First Issue", DateTime.UtcNow.AddDays(-2));
        var issue2 = new Issue(repositoryId, "Second Issue", DateTime.UtcNow.AddDays(-1));

        // Act
        _context.GitRepositories.Add(repository);
        _context.Issues.AddRange(issue1, issue2);
        await _context.SaveChangesAsync();

        // Assert
        var repositoryCount = await _context.GitRepositories.CountAsync();
        var issueCount = await _context.Issues.CountAsync();
        
        Assert.Equal(1, repositoryCount);
        Assert.Equal(2, issueCount);
        
        var issues = await _context.Issues
            .Where(i => i.GitRepositoryId == repositoryId)
            .ToListAsync();
        
        Assert.Equal(2, issues.Count);
        Assert.Contains(issues, i => i.Name == "First Issue");
        Assert.Contains(issues, i => i.Name == "Second Issue");
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }
}
