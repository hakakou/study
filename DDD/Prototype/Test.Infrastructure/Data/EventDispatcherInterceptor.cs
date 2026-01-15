using Haka.Patterns.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Test.Infrastructure.Data;

/// <summary>
/// Intercepts SaveChanges to dispatch domain events after changes are successfully saved 
/// </summary>
public class EventDispatchInterceptor(IDomainEventDispatcher domainEventDispatcher) : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;

    // Called after SaveChangesAsync has completed successfully
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
      CancellationToken cancellationToken = new CancellationToken())
    {
        var context = eventData.Context!;

        // Retrieve all tracked entities that have domain events
        var entitiesWithEvents = context.ChangeTracker.Entries<HasDomainEventsBase>()
          .Select(e => e.Entity)
          .Where(e => e.DomainEvents.Count > 0)
          .ToArray();

        // Dispatch and clear domain events
        await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}

