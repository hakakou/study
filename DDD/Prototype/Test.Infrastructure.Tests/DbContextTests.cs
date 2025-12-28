using AwesomeAssertions;
using Haka.Patterns.DDD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Domain.AggregateModel;
using Test.Infrastructure.Data;
using Xunit;
using static Test.Domain.AggregateModel.IssuePredicates;

namespace Test.Infrastructure.Tests;

[Collection("DI")]
public class DbContextTests : IAsyncLifetime
{
    private readonly TestDbContext _dbContext;
    private readonly IRepository<Repo> _gitRepositoryRepository;
    private readonly IRepository<Issue> _issueRepository;
    private readonly IServiceScope _scope;

    public DbContextTests(DatabaseFixture fixture)
    {
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
        _gitRepositoryRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Repo>>();
        _issueRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Issue>>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => _scope.Dispose();

    [Fact]
    public async Task CanInsertGitRepository()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };

        var issue1 = new Issue(repositoryId, "Issue 1", DateTime.UtcNow);

        // Act
        repository.AddIssue(issue1);
        await _gitRepositoryRepository.AddAsync(repository);

        // Assert
        var savedRepository = await _gitRepositoryRepository
            .FirstOrDefaultAsync(r => r.Id == repositoryId);

        var r = new ReposWithManyIssues();
        r.lsSatisfiedBy(repository).Should().BeTrue();

        Assert.Equal("TestRepository", savedRepository.Name);
        Assert.Equal(1, savedRepository.Issues.Count());
    }

    [Fact]
    public async Task CanInsertIssue()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _gitRepositoryRepository.AddAsync(repository);
        await _gitRepositoryRepository.SaveChangesAsync();

        var createdDate = DateTime.UtcNow;
        var issue = new Issue(repositoryId, "Test Issue", createdDate);
        issue.Description = "This is a test issue";

        // Act
        await _issueRepository.AddAsync(issue);
        await _issueRepository.SaveChangesAsync();

        // Assert
        var savedIssue = await _dbContext.Issues
            .FirstOrDefaultAsync(i => i.Name == "Test Issue");
        
        Assert.NotNull(savedIssue);
        Assert.Equal("Test Issue", savedIssue.Name);
        Assert.Equal("This is a test issue", savedIssue.Description);
        Assert.Equal(repositoryId, savedIssue.RepoId);
        Assert.Equal(createdDate, savedIssue.CreatedDate);
    }

    [Fact]
    public async Task CanInsertMultipleEntities()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new Repo(repositoryId, "MainRepository") { Name = "MainRepository" };
        
        var issue1 = new Issue(repositoryId, "First Issue", DateTime.UtcNow.AddDays(-2));
        var issue2 = new Issue(repositoryId, "Second Issue", DateTime.UtcNow.AddDays(-1));

        // Act
        await _gitRepositoryRepository.AddAsync(repository);
        await _issueRepository.AddAsync(issue1);
        await _issueRepository.AddAsync(issue2);

        // Assert
        var repositoryCount = await _dbContext.GitRepositories.CountAsync();
        var issueCount = await _dbContext.Issues.CountAsync();
        
        Assert.Equal(1, repositoryCount);
        Assert.Equal(2, issueCount);
        
        var issues = await _dbContext.Issues
            .Where(i => i.RepoId == repositoryId)
            .ToListAsync();
        
        Assert.Equal(2, issues.Count);
        Assert.Contains(issues, i => i.Name == "First Issue");
        Assert.Contains(issues, i => i.Name == "Second Issue");
    }
}
