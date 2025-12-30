using Haka.Patterns.DDD;

namespace Test.Domain.AggregateModel;

public class Issue : EntityBase<Issue, int>, IAggregateRoot
{
    internal Issue(Guid repoId, IssueName name, DateTime createdDate) : base()
    {
        RepoId = repoId;
        CreatedDate = createdDate;
        Name = name;
    }

    public Guid RepoId { get; private set; }
    public IssueName Name { get; private set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; private set; }

    public void SetName(IssueName name)
    {
        Name = name;
    }

    public Guid? AssignedUserId { get; internal set; }
}

public readonly record struct IssueName(string Value);