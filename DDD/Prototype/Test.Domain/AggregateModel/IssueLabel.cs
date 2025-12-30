using Haka.Patterns.DDD;

namespace Test.Domain.AggregateModel;

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
