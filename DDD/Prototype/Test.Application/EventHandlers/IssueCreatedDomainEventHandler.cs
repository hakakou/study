using Mediator;
using Microsoft.Extensions.Logging;
using Test.Domain.Events;

namespace Test.Application.EventHandlers;

public sealed class IssueCreatedDomainEventHandler : INotificationHandler<IssueCreatedDomainEvent>
{
    private readonly ILogger<IssueCreatedDomainEventHandler> _logger;

    public IssueCreatedDomainEventHandler(ILogger<IssueCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(IssueCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Issue created: IssueId={IssueId}, RepoId={RepoId}, Name={IssueName}, CreatedDate={CreatedDate}",
            notification.IssueId,
            notification.RepoId,
            notification.IssueName,
            notification.CreatedDate);

        return ValueTask.CompletedTask;
    }
}
