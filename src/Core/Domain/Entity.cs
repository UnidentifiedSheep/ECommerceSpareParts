using Domain.Events;
using Domain.Interfaces;
using Domain.Interfaces.Events;

namespace Domain;

public abstract class Entity<TModel, TKey> : IEntity<TKey>
	where TModel : Entity<TModel, TKey> where TKey : notnull
{
	private readonly List<IDomainEvent> _domainEvents = [];

	private readonly Dictionary<string, IDomainEvent> _keyedDomainEvents = [];

	public abstract TKey GetId();

	object IEntity.GetId() => GetId();

	public IReadOnlyCollection<IDomainEvent> FlushDomainEvents()
	{
		var result = new List<IDomainEvent>(_domainEvents.Count + _keyedDomainEvents.Count);

		result.AddRange(_domainEvents);
		result.AddRange(_keyedDomainEvents.Values);

		_domainEvents.Clear();
		_keyedDomainEvents.Clear();

		return result;
	}

	public virtual void OnDeleted()
	{
		if (this is IGenerateAutomaticDomainEvents)
			AddEntityDeleteDomainEvent();
	}

	public virtual void OnUpdated()
	{
		if (this is IGenerateAutomaticDomainEvents)
			AddEntityUpdateDomainEvent();
	}

	public virtual void OnCreated()
	{
		if (this is IGenerateAutomaticDomainEvents)
			AddEntityCreateDomainEvent();
	}

	protected void AddDomainEvent(IDomainEvent domainEvent)
	{
		ArgumentNullException.ThrowIfNull(domainEvent);

		if (domainEvent is IKeyedDomainEvent keyed)
			_keyedDomainEvents[keyed.GetKey()] = domainEvent;
		else
			_domainEvents.Add(domainEvent);
	}

	protected void AddEntityCreateDomainEvent() =>
		AddDomainEvent(new EntityCreatedDomainEvent<TModel>((TModel)this));

	protected void AddEntityUpdateDomainEvent() =>
		AddDomainEvent(new EntityUpdatedDomainEvent<TModel, TKey>(GetId()));

	protected void AddEntityDeleteDomainEvent() =>
		AddDomainEvent(new EntityDeletedDomainEvent<TModel, TKey>(GetId()));
}
