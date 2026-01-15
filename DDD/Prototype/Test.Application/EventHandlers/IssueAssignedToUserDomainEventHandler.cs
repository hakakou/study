using Mediator;
using Microsoft.Extensions.Logging;
using Test.Domain.Events;

namespace Test.Application.EventHandlers;

public sealed class IssueAssignedToUserDomainEventHandler : INotificationHandler<IssueAssignedToUserDomainEvent>
{
    private readonly ILogger<IssueAssignedToUserDomainEventHandler> _logger;

    public IssueAssignedToUserDomainEventHandler(ILogger<IssueAssignedToUserDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(IssueAssignedToUserDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Issue assigned to user: IssueId={IssueId}, UserId={UserId}, RepoId={RepoId}, IssueName={IssueName}",
            notification.IssueId,
            notification.UserId,
            notification.RepoId,
            notification.IssueName);

        return ValueTask.CompletedTask;
    }
}
