using Haka.Patterns.DDD;
using Test.Domain.Specifications;

namespace Test.Domain.AggregateModel;

public class Issue : EntityBase<Issue, int>, IAggregateRoot
{
    internal Issue(Guid repoId, IssueName name, DateTime createdDate) : base()
    {
        RepoId = repoId;
        CreatedDate = createdDate;
        Name = name;
        Labels = [];
    }

    public Guid RepoId { get; private set; }
    public IssueName Name { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string? Description { get; set; }
    public Guid? AssignedUserId { get; internal set; }
    public ICollection<IssueLabel> Labels { get; private set; }

    public void SetName(IssueName name)
    {
        Name = name;
    }

    public void AddLabel(IssueLabel label)
    {
        Labels.Add(label);
    }


    public bool IsInInactive()
    {
        return new InactiveIssueSpecification().IsSatisfiedBy(this);
    }
}

public readonly record struct IssueName(string Value);