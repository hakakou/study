using Mediator;

namespace Haka.Patterns.DDD;

public interface IDomainEvent : INotification
{
  DateTime DateOccurred { get; }
}
