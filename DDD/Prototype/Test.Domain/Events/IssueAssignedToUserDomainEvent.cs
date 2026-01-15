using Mediator;

namespace Test.Domain.Events;

/// <summary>
/// Domain event raised when an issue is assigned to a user.
/// This event can trigger notifications to the assigned user, update their workload metrics,
/// or log assignment history.
/// </summary>
public sealed class IssueAssignedToUserDomainEvent : INotification
{
    public IssueAssignedToUserDomainEvent(Guid issueId, Guid userId, Guid repoId, string issueName)
    {
        IssueId = issueId;
        UserId = userId;
        RepoId = repoId;
        IssueName = issueName;
        DateOccurred = DateTime.UtcNow;
    }

    public Guid IssueId { get; }
    public Guid UserId { get; }
    public Guid RepoId { get; }
    public string IssueName { get; }
    public DateTime DateOccurred { get; }
}
