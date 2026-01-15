using System;
using Mediator;

namespace Test.Domain.Events;

/// <summary>
/// Domain event raised when a new issue is created.
/// This event can trigger side effects such as notifications, analytics tracking, 
/// or updating related aggregates.
/// </summary>
public sealed class IssueCreatedDomainEvent : INotification
{
    public IssueCreatedDomainEvent(Guid issueId, Guid repoId, string issueName, DateTime createdDate)
    {
        IssueId = issueId;
        RepoId = repoId;
        IssueName = issueName;
        CreatedDate = createdDate;
        DateOccurred = DateTime.UtcNow;
    }

    public Guid IssueId { get; }
    public Guid RepoId { get; }
    public string IssueName { get; }
    public DateTime CreatedDate { get; }
    public DateTime DateOccurred { get; }
}
