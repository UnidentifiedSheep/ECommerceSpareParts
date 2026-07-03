using Application.Common.Interfaces.Events;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors;

public class DomainEventFlusherSaveChangesInterceptor(
    IDomainEventScope domainEventScope) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CollectDomainEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CollectDomainEvents(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void CollectDomainEvents(DbContext? context)
    {
        if (context is null) return;

        var entities = context.ChangeTracker
            .Entries<IEntity>()
            .Select(x => (x.Entity, x))
            .ToArray();

        foreach (var (entity, entry) in entities)
        {
            if (entry.State == EntityState.Deleted) entity.OnDeleted();
            
            var events = entity.FlushDomainEvents();
            if (domainEventScope.IsCollectionEnabled)
                domainEventScope.AddRange(events);
        }
    }
}
