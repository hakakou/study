using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Clean.Core.ContributorAggregate;
using eShop.Ordering.Domain.Exceptions;

namespace Test.Domain.AggregateModel;

public class GitRepository : EntityBase<Guid>, IAggregateRoot
{
  public required string Name { get; set; }

  public GitRepository(Guid id, string name) : base()
  {
    Id = id;
    Name = name;
  }
}

public class Issue : EntityBase<Issue, int>, IAggregateRoot
{
  public Issue(Guid gitRepositoryId, IssueName name, DateTime createdDate) : base()
  {
    GitRepositoryId = gitRepositoryId;
    CreatedDate = createdDate;
    Name = name;
    //Labels = [];
  }

  public Guid GitRepositoryId { get; private set; }
  public IssueName Name { get; private set; }
  public string? Description { get; set; }
  public DateTime CreatedDate { get; private set; }
  //public ICollection<IssueLabel> Labels { get; private set; }

  public bool IsInInactive()
  {
    return new InactiveIssuesSpecification().IsSatisfiedBy(this);
  }

  public void SetName(IssueName name)
  {
    Name = name;
  }

  public Guid? AssignedUserId { get; internal set; }

}

public class IssueManager (IRepository<Issue> issueRepository)
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

public class InactiveIssuesSpecification : Specification<Issue>
{
  public InactiveIssuesSpecification()
  {
    Query.IsOld()
      .Where(issue => issue.AssignedUserId == null);
  }
}

public class IssueNameSpec : Specification<Issue, IssueName>
{
  public IssueNameSpec(int id)
  {
    Query
        .Where(x => x.Id == id)
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
