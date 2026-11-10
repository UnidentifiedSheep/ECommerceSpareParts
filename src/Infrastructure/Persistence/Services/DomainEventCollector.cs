using Application.Common.Interfaces.Events;
using Domain.Interfaces;
using Domain.Interfaces.Events;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Services;

public sealed class DomainEventCollector<TContext>(
	IDomainEventScope eventScope,
	Func<TContext> contextFactory)
	: IDomainEventCollector where TContext : DbContext
{
	private readonly Dictionary<IEntity, EntityState> _states = new(ReferenceEqualityComparer.Instance);

	public void Collect()
	{
		var context = contextFactory();
		context.ChangeTracker.DetectChanges();

		foreach (var entry in context.ChangeTracker.Entries<IEntity>())
		{
			if (entry.State == EntityState.Detached) continue;
			if (!_states.TryGetValue(entry.Entity, out var previous) || previous != entry.State)
			{
				_states[entry.Entity] = entry.State;
				switch (entry.State)
				{
					case EntityState.Added:
						entry.Entity.OnCreated();
						break;
					case EntityState.Modified when entry.Properties.Any(property => property.IsModified):
						entry.Entity.OnUpdated();
						break;
					case EntityState.Deleted:
						entry.Entity.OnDeleted();
						break;
				}
			}

			foreach (var item in entry.Entity.FlushDomainEvents())
				if (eventScope.IsCollectionEnabled || item is IBeforeSaveDomainEvent)
					eventScope.Add(item);
		}
	}

	public void Reset() => _states.Clear();
}
