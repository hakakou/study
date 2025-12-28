namespace Haka.Patterns.DDD;

public interface IHasDomainEvents
{
  IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
  void ClearDomainEvents();
}
