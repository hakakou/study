using AwesomeAssertions;
using Haka.Patterns.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Domain.AggregateModel.AppUserAggregate;
using Test.Domain.AggregateModel.IssueAggregate;
using Test.Domain.AggregateModel.RepoAggregate;
using Test.Domain.DomainServices;
using Test.Domain.Specifications;
using Test.Infrastructure.Data;
using Xunit;

namespace Test.Infrastructure.Tests;

[Collection("DI")]
public class Specification2Tests : IAsyncLifetime
{
    private readonly TestDbContext _dbContext;
    private readonly IRepository<Repo> _repoRepository;
    private readonly IRepository<Issue> _issueRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IServiceScope _scope;
    private readonly SqlCommandInterceptor _sql;

    public Specification2Tests(DatabaseFixture fixture)
    {
        _sql = fixture.SqlInterceptor;
        _scope = fixture.ServiceProvider.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
        _repoRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Repo>>();
        _issueRepository = _scope.ServiceProvider.GetRequiredService<IRepository<Issue>>();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IRepository<User>>();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => _scope.Dispose();

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

        var list = _dbContext.ApplySpecification(spec);
        list.Should().ContainSingle();
    }
}
