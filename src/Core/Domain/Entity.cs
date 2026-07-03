using Domain.Interfaces;
using Domain.Interfaces.Events;

namespace Domain;

public abstract class Entity<TModel, TKey>
    : IEntity<TKey> where TModel : Entity<TModel, TKey> where TKey : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly Dictionary<string, IDomainEvent> _keyedDomainEvents = [];
    public abstract TKey GetId();

    object IEntity.GetId() { return GetId(); }

    public IReadOnlyCollection<IDomainEvent> FlushDomainEvents()
    {
        var result = new List<IDomainEvent>(
            _domainEvents.Count + _keyedDomainEvents.Count);

        result.AddRange(_domainEvents);
        result.AddRange(_keyedDomainEvents.Values);

        _domainEvents.Clear();
        _keyedDomainEvents.Clear();

        return result;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (domainEvent is IKeyedDomainEvent keyed)
            _keyedDomainEvents[keyed.GetKey()] = domainEvent;
        else
            _domainEvents.Add(domainEvent);
    }

    public virtual void OnDeleted() { }
    public virtual void OnUpdated() { }
    public virtual void OnCreated() { }
}