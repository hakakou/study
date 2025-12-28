using Mediator;

namespace Haka.Patterns.DDD;

public interface IDomainEventHandler<T> : INotificationHandler<T> where T : IDomainEvent
{
}
