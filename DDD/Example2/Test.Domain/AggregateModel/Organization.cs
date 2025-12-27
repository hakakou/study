using eShop.Ordering.Domain.Exceptions;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Test.Domain.SeedWork;

namespace Test.Domain.AggregateModel;

public class GitRepository : Entity, IAggregateRoot
{
    public required string Name { get; set; }

    public GitRepository(Guid id, string name) : base()
    {
        Id = id;
        Name = name;
    }

    private GitRepository() { }
}

public class Issue : Entity, IAggregateRoot
{
    public Issue(Guid id, Guid gitRepositoryId, string name) : base()
    {
        Id = id;
        GitRepositoryId = gitRepositoryId;

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Issue name cannot be null or empty.", nameof(name));

        Name = name;
        Labels = [];
    }
    private Issue() { }

    public Guid GitRepositoryId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; set; }
    public ICollection<IssueLabel> Labels { get; private set; }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Issue name cannot be null or empty.", nameof(name));
        Name = name;
    }
    
    public Guid? AssignedUserId { get; private set; }

    public async Task AssignToUser(AppUser user, IUserIssueService userIssueService)
    {
        var issues = await userIssueService.GetOpenIssuesCount(user);
        if (issues >= 3)
            throw new BusinessException("...");
        
        AssignedUserId = user.Id;
    }
}

public interface IUserIssueService
{
    Task<int> GetOpenIssuesCount(AppUser user);
}

public class AppUser : Entity, IAggregateRoot
{
    public AppUser(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
    }
    private AppUser() { }
    public string UserName { get; private set; }
}

public class IssueLabel : Entity
{
    public IssueLabel(Guid id, Guid issueId, string name) : base()
    {
        Id = id;
        IssueId = issueId;
        Name = name;
    }
    public Guid IssueId { get; set; }
    public string Name { get; private set; }
}
