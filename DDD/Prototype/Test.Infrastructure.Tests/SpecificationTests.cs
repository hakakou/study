using AwesomeAssertions;
using Haka.Patterns.DDD;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Domain.AggregateModel;
using Test.Domain.DomainServices;
using Test.Domain.Specifications;
using Test.Infrastructure.Data;
using Xunit;

namespace Test.Infrastructure.Tests;

[Collection("DI")]
public class SpecificationTests : IAsyncLifetime
{
    private readonly TestDbContext _dbContext;
    private readonly IRepository<Repo> _repoRepository;
    private readonly IRepository<Issue> _issueRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IServiceScope _scope;
    private readonly SqlCommandInterceptor _sql;

    public SpecificationTests(DatabaseFixture fixture)
    {
        _sql = fixture.SqlInterceptor;
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
        _repoRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Repo>>();
        _issueRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Issue>>();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IRepository<User>>();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => _scope.Dispose();

    [Fact]
    public async Task TrendingIssues_WithAssignedIssues_FiltersByAssignment()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var userId = Guid.NewGuid();
        var user = new User(userId, "TestUser");
        await _userRepository.AddAsync(user);

        var issueManager = new IssueManager(_issueRepository);
        
        var assignedIssue = await issueManager
            .CreateAsync(repositoryId, new IssueName("Assigned Issue"), DateTime.UtcNow);
        await issueManager.AssignToUser(user, assignedIssue);
        await _issueRepository.AddAsync(assignedIssue);

        var unassignedIssue = await issueManager
            .CreateAsync(repositoryId, new IssueName("Unassigned Issue"), DateTime.UtcNow);
        await _issueRepository.AddAsync(unassignedIssue);

        // Act
        _dbContext.ChangeTracker.Clear();
        var spec = new TrendingIssues();

        // Use spec's IsSatisfiedBy method directly
        var allIssues = await _dbContext.Issues.Where(i => i.RepoId == repositoryId).ToListAsync();
        var result = allIssues.Where(i => spec.IsSatisfiedBy(i)).ToList();

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be(new IssueName("Assigned Issue"));
        spec.IsSatisfiedBy(assignedIssue).Should().BeTrue();
        spec.IsSatisfiedBy(unassignedIssue).Should().BeFalse();
    }

    [Fact]
    public async Task InactiveIssueSpecification_WithUnassignedIssues_ReturnsMatchingIssues()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var userId = Guid.NewGuid();
        var user = new User(userId, "TestUser");
        await _userRepository.AddAsync(user);

        var issueManager = new IssueManager(_issueRepository);
        
        var assignedIssue = await issueManager
            .CreateAsync(repositoryId, new IssueName("Assigned Issue"), DateTime.UtcNow);
        await issueManager.AssignToUser(user, assignedIssue);
        await _issueRepository.AddAsync(assignedIssue);

        var unassignedIssue = await issueManager
            .CreateAsync(repositoryId, new IssueName("Unassigned Issue"), DateTime.UtcNow);
        await _issueRepository.AddAsync(unassignedIssue);

        // Act
        _dbContext.ChangeTracker.Clear();
        var spec = new InactiveIssueSpecification();
        var result = await _issueRepository.ListAsync(spec, CancellationToken.None);

        // Assert
        result.Should().ContainSingle();
        result.First().Name.Should().Be(new IssueName("Unassigned Issue"));
        spec.IsSatisfiedBy(unassignedIssue).Should().BeTrue();
        spec.IsSatisfiedBy(assignedIssue).Should().BeFalse();
    }

    [Fact]
    public async Task DuplicateNameSpecification_WithMatchingIssue_ReturnsTrueForSatisfiedBy()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var issueName = new IssueName("Duplicate Issue");
        var issue = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, issueName, DateTime.UtcNow);
        await _issueRepository.AddAsync(issue);

        // Act
        _sql.Clear();
        var spec = new DuplicateNameSpecification(repositoryId, issueName);

        // Assert
        spec.IsSatisfiedBy(issue).Should().BeTrue();
        _sql.Commands.Should().HaveCount(0);
    }

    [Fact]
    public async Task IssueNameSpec_WithMatchingId_ReturnsIssueName()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var issueName = new IssueName("Specific Issue");
        var issue = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, issueName, DateTime.UtcNow);
        await _issueRepository.AddAsync(issue);

        // Act
        _dbContext.ChangeTracker.Clear();
        var spec = new IssueNameSpec(issue.Id);
        var result = await _issueRepository.FirstOrDefaultAsync(spec, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(issueName.Value);
    }

    [Fact]
    public async Task FirstOrDefaultSpecification_WithMultipleIssues_ReturnsSingleResult()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var repo = new Repo(repositoryId, "TestRepository") { Name = "TestRepository" };
        await _repoRepository.AddAsync(repo);

        var issue1 = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, new IssueName("Issue 1"), DateTime.UtcNow);
        await _issueRepository.AddAsync(issue1);

        var issue2 = await new IssueManager(_issueRepository)
            .CreateAsync(repositoryId, new IssueName("Issue 2"), DateTime.UtcNow);
        await _issueRepository.AddAsync(issue2);

        // Act
        _dbContext.ChangeTracker.Clear();
        var spec = new FirstOrDefaultSpecification<Issue>();
        var result = await _issueRepository.FirstOrDefaultAsync(spec, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

}
