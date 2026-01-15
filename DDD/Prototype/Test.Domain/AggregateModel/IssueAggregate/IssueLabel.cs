using Ardalis.GuardClauses;
using Haka.Patterns.SeedWork;

namespace Test.Domain.AggregateModel.IssueAggregate;

public class IssueLabel : EntityBase<Guid>
{
    public IssueLabel(Guid id, Guid issueId, string name) : base()
    {
        Id = id;
        IssueId = issueId;
        Guard.Against.NullOrWhiteSpace(name);
        Name = name;
    }
    public Guid IssueId { get; set; }
    public string Name { get; private set; }
}
