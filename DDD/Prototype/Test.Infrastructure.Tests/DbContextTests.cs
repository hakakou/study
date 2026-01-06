using AwesomeAssertions;
using Haka.Patterns.DDD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Domain.AggregateModel;
using Test.Domain.DomainServices;
using Test.Domain.Exceptions;
using Test.Infrastructure.Data;
using Xunit;
using Xunit.Abstractions;

namespace Test.Infrastructure.Tests;

[Collection("DI")]
public class DbContextTests : IAsyncLifetime
{
    private readonly TestDbContext _dbContext;
    private readonly IRepository<Repo> _repoRepository;
    private readonly IRepository<Issue> _issueRepository;
    private readonly IServiceScope _scope;
    private readonly SqlCommandInterceptor _sql;
    private readonly ITestOutputHelper _output;

    public DbContextTests(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _sql = fixture.SqlInterceptor;
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
        _repoRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Repo>>();
        _issueRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Issue>>();
        _output = output;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => _scope.Dispose();

    [Fact]
    public async Task Schema_Test()
    {
        var repo = new Repo("TestRepository") { Name = "TestRepository" };

        await _repoRepository.AddAsync(repo);

        foreach (var command in _sql.Commands)
            _output.WriteLine(command);
    }

    [Fact]
    public async Task AddRepo_WithIssue_InsertsToDatabase()
    {
        _sql.Commands.Clear();


        // Arrange
        var repo = new Repo("TestRepository") { Name = "TestRepository" };
        repo.Id.Should().Be(Guid.Empty);

        repo.AddRepoItem("src/Issue1.cs");
        var r = repo.AddRepoItem("src/Issue2.cs");

        // Act - Application layer saves
        await _repoRepository.AddAsync(repo);
        var repositoryId = repo.Id;
        repo.Id.Should().NotBe(Guid.Empty);

        r.Repo.Should().Be(repo);

        var issue1 = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, new IssueName("Issue 1"), DateTime.UtcNow);

        await _issueRepository.AddAsync(issue1);

        // Assert
        _sql.Commands.Should().ContainMatch("INSERT INTO *");
    }

    [Fact]
    public async Task CreateIssue_WithDuplicateName_ThrowsBusinessException()
    {
        // Arrange
        var repo = new Repo("TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var issueName = new IssueName("Duplicate Issue");
        var issue1 = await new IssueManager(_issueRepository)
            .CreateAsync(repo.Id, issueName, DateTime.UtcNow);
        await _issueRepository.AddAsync(issue1);

        // Act & Assert
        Func<Task> command = async () =>
            await new IssueManager(_issueRepository)
                .CreateAsync(repo.Id, issueName, DateTime.UtcNow);

        await command.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task AddIssue_WithAllProperties_PersistsCorrectly()
    {
        // Arrange
        var repository = new Repo("TestRepository") { Name = "TestRepository" };

        await _repoRepository.AddAsync(repository);

        var createdDate = DateTime.UtcNow;
        var issue = await new IssueManager(_issueRepository)
            .CreateAsync(repository.Id, new IssueName("Test Issue"), createdDate);

        issue.Description = "This is a test issue";

        // Act - Application layer saves
        await _issueRepository.AddAsync(issue);

        // Assert
        var savedIssue = await _dbContext.Issues
            .FirstOrDefaultAsync(i => i.Name == new IssueName("Test Issue"));

        Assert.NotNull(savedIssue);
        Assert.Equal(new IssueName("Test Issue"), savedIssue.Name);
        Assert.Equal("This is a test issue", savedIssue.Description);
        Assert.Equal(repository.Id, savedIssue.RepoId);
        Assert.Equal(createdDate, savedIssue.CreatedDate);
    }

    [Fact]
    public async Task AddMultipleIssues_ToSameRepo_AllPersistCorrectly()
    {
        var test = Guid.CreateVersion7();

        // Arrange
        var repositoryId = Guid.NewGuid();
        var repository = new Repo(repositoryId, "MainRepository") { Name = "MainRepository" };

        var issue1 = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, new IssueName("First Issue"), DateTime.UtcNow.AddDays(-2));
        var issue2 = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, new IssueName("Second Issue"), DateTime.UtcNow.AddDays(-1));

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
        Assert.Contains(issues, i => i.Name == new IssueName("First Issue"));
        Assert.Contains(issues, i => i.Name == new IssueName("Second Issue"));
    }
}