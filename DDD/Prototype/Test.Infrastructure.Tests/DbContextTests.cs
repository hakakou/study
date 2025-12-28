using AwesomeAssertions;
using Haka.Patterns.DDD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Domain.AggregateModel;
using Test.Infrastructure.Data;
using Xunit;

namespace Test.Infrastructure.Tests;

[Collection("DI")]
public class DbContextTests : IAsyncLifetime
{
    private readonly TestDbContext _dbContext;
    private readonly IRepository<Repo> _repoRepository;
    private readonly IRepository<Issue> _issueRepository;
    private readonly IServiceScope _scope;
    private readonly SqlCommandInterceptor _sql;

    public DbContextTests(DatabaseFixture fixture)
    {
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
        _sql = fixture.SqlInterceptor;
        _repoRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Repo>>();
        _issueRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Issue>>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => _scope.Dispose();

    [Fact]
    public async Task CanInsertGitRepository()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };

        var issue1 = new Issue(repositoryId, "Issue 1", DateTime.UtcNow);

        // Act
        _sql.Commands.Clear();
        repo.AddIssue(issue1);
        await _repoRepository.AddAsync(repo);

        _sql.Commands.Should().ContainMatch("INSERT INTO *");

        // Assert
        _sql.Commands.Clear();
        _dbContext.ChangeTracker.Clear();

        var repo2 = await _dbContext.Repos
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.Id == repositoryId);

        // Specification Type 1
        var spec1 = new ReposWithManyIssues();
        var rQuery = _dbContext.ApplySpecification(spec1);
        rQuery.Should().ContainSingle();
        spec1.IsSatisfiedBy(repo).Should().BeTrue();

        var list1 = await _repoRepository.WhereAsync(spec1, CancellationToken.None);
        list1.Should().ContainSingle();

        // Specification Type 2
        var spec2 = new InactiveRepoSpecification();
        spec2.IsSatisfiedBy(repo2).Should().BeFalse();

        var list2 = await _repoRepository.ListAsync(spec2);
        list2.Should().BeEmpty();

        Assert.Equal("TestRepository", repo2.Name);
        Assert.Equal(1, repo2.Issues.Count());
    }

    [Fact]
    public async Task CanInsertIssue()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repository);
        await _repoRepository.SaveChangesAsync();

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
        await _repoRepository.AddAsync(repository);
        await _issueRepository.AddAsync(issue1);
        await _issueRepository.AddAsync(issue2);

        // Assert
        var repositoryCount = await _dbContext.Repos.CountAsync();
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
