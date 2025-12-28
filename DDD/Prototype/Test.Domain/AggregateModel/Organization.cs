using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Ardalis.Specification;
using Haka.Patterns.DDD;

namespace Test.Domain.AggregateModel;

public class Repo : EntityBase<Guid>, IAggregateRoot
{
    public required string Name { get; set; }

    public ICollection<Issue> Issues { get; private set; }

    public void AddIssue(Issue issue)
    {
        Issues.Add(issue);
    }

    public Repo(Guid id, string name) : base()
    {
        Id = id;
        Name = name;
        Issues = new Collection<Issue>();
    }

    public bool IsInInactive()
    {
        return new InactiveRepoSpecification().IsSatisfiedBy(this);
    }
}

public class Issue : EntityBase<Issue, int>, IAggregateRoot
{
    public Issue(Guid repoId, string name, DateTime createdDate) : base()
    {
        RepoId = repoId;
        CreatedDate = createdDate;
        Name = name;
        //Labels = [];
    }

    public Guid RepoId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; private set; }
    //public ICollection<IssueLabel> Labels { get; private set; }

    public void SetName(string name)
    {
        Name = name;
    }

    public Guid? AssignedUserId { get; internal set; }
}



public class IssueManager(IRepository<Issue> issueRepository)
{
    public async Task AssignToUser(AppUser user, Issue issue)
    {
        var issues = await issueRepository.CountAsync();
        if (issues >= 3)
            throw new BusinessException("...");

        issue.AssignedUserId = user.Id;
    }
}

public interface IUserIssueService
{
    Task<int> GetOpenIssuesCount(AppUser user);
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class AppUser : EntityBase<Guid>, IAggregateRoot
{
    public AppUser(Guid id, string userName) : base()
    {
        Id = id;
        UserName = userName;
    }

    public string UserName { get; private set; }
}

public class IssueLabel : EntityBase<Guid>
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

public class InactiveRepoSpecification : Specification<Repo>
{
    public InactiveRepoSpecification()
    {
        Query.Where(issue => !issue.Issues.Any());
    }
}

public class IssueNameSpec : Specification<Issue, string>
{
    public IssueNameSpec(int id)
    {
        Query
            .Where(x => x.Id == id)
            .OrderBy(x => x.Name)
            .Select(x => x.Name);
    }
}

public class FirstOrDefaultSpecification<T> : SingleResultSpecification<T>
{
    public FirstOrDefaultSpecification()
    {
        Query.Take(1);
    }
}

public static class IssuePredicates
{
    public static ISpecificationBuilder<Issue> IsOld(this ISpecificationBuilder<Issue> builder)
    {
        var dateThreshold = DateTime.UtcNow.AddDays(-30);
        return builder.Where(x => x.CreatedDate < dateThreshold);
    }
}

public class ReposWithManyIssues : Haka.Patterns.Specifications.Specification<Repo>
{
    public ReposWithManyIssues(int min = 1)
    {
        AddFilteringQuery(repo => repo.Issues.Count >= min);
        AddOrderByDescendingQuery(repo => repo.Issues.Count);
        AddIncludeQuery(repo => repo.Issues);
    }
}
