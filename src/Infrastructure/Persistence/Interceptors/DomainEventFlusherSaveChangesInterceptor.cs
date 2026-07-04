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

        foreach (var entry in context.ChangeTracker.Entries<IEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.OnCreated();
                    break;

                case EntityState.Modified when entry.Properties.Any(p => p.IsModified):
                    entry.Entity.OnUpdated();
                    break;

                case EntityState.Deleted:
                    entry.Entity.OnDeleted();
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged: //no changes -> no events. Нету ручек - нет конфетки
                    continue;
            }
            
            var events = entry.Entity.FlushDomainEvents();
            if (domainEventScope.IsCollectionEnabled)
                domainEventScope.AddRange(events);
        }
    }
}
