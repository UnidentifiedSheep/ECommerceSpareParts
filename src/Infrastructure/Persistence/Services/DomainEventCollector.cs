using System.Runtime.CompilerServices;
using Application.Common.Interfaces.Events;
using Domain.Interfaces;
using Domain.Interfaces.Events;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.Services;

public sealed class DomainEventCollector(IDomainEventScope eventScope) : IDomainEventCollector
{
	private readonly ConditionalWeakTable<DbContext, Dictionary<IEntity, EntityState>> _collected = new();

	public void Collect(DbContext? context)
	{
		if (context is null) return;
		context.ChangeTracker.DetectChanges();
		var states = _collected.GetValue(context, _ => new(ReferenceEqualityComparer.Instance));

		foreach (var entry in context.ChangeTracker.Entries<IEntity>())
		{
			if (entry.State == EntityState.Detached) continue;
			if (!states.TryGetValue(entry.Entity, out var previous) || previous != entry.State)
			{
				states[entry.Entity] = entry.State;
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

	public void Reset(DbContext context) => _collected.Remove(context);
}
