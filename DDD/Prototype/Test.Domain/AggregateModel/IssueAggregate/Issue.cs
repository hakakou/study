using Test.Domain.Specifications;
using Test.Domain.Events;
using Haka.Patterns.SeedWork;

namespace Test.Domain.AggregateModel.IssueAggregate;

public class Issue : EntityBase<Guid>, IAggregateRoot
{
    internal Issue(Guid repoId, IssueName name, DateTime createdDate) : base()
    {
        RepoId = repoId;
        CreatedDate = createdDate;
        Name = name;
        _labels = [];
        
        // Raise domain event when issue is created
        RegisterDomainEvent(new IssueCreatedDomainEvent(Id, repoId, name.Value, createdDate));
    }

    private DateTime _updatedDate;

    public Guid RepoId { get; private set; }
    public IssueName Name { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public string? Description { get; set; }
    public Guid? AssignedUserId { get; internal set; }

    private readonly List<IssueLabel> _labels;
    public IReadOnlyCollection<IssueLabel> Labels => _labels;
   
    // The above is better then
    //public ICollection<IssueLabel> Labels { get; private set; }

    public void SetName(IssueName name)
    {
        Name = name;
    }

    public void AddLabel(IssueLabel label)
    {
        _labels.Add(label);
    }

    public bool IsInInactive()
    {
        return new InactiveIssueSpecification().IsSatisfiedBy(this);
    }
    
    public void AssignToUser(Guid userId)
    {
        AssignedUserId = userId;
        
        // Raise domain event when issue is assigned
        RegisterDomainEvent(new IssueAssignedToUserDomainEvent(Id, userId, RepoId, Name.Value));
    }
}

public readonly record struct IssueName(string Value);