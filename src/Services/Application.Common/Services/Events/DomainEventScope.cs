using System.Collections.Concurrent;
using System.Linq.Expressions;
using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;
using MediatR;

namespace Application.Common.Services.Events;

public class DomainEventScope(
    IPublisher publisher) : IDomainEventScope
{
    private static readonly ConcurrentDictionary<Type, Func<IBatchBucket>> BucketFactories = new();

    private readonly Dictionary<Type, IBatchBucket> _buckets = [];
    private readonly List<IDomainEvent> _events = [];
    private readonly Dictionary<string, IDomainEvent> _keyedEvents = new();
    private int _collectionDepth;

    public bool IsCollectionEnabled => _collectionDepth > 0;

    public IDisposable EnableCollection()
    {
        _collectionDepth++;
        return new CollectionScope(this);
    }

    public Task PublishImmediatelyAsync<T>(
        T @event,
        CancellationToken ct = default) where T : IDomainEvent
    {
        return publisher.Publish(@event, ct);
    }

    public void Add<T>(T @event) where T : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        switch (@event)
        {
            case IBatchableDomainEvent batchable:
                AddBatchable(batchable);
                break;
            case IKeyedDomainEvent ke:
                _keyedEvents[ke.GetKey()] = @event;
                break;
            default:
                _events.Add(@event);
                break;
        }
    }

    public void AddRange<T>(IEnumerable<T> events) where T : IDomainEvent
    {
        foreach (var @event in events) Add(@event);
    }

    public IReadOnlyCollection<IDomainEvent> Flush()
    {
        var result = new List<IDomainEvent>(
            _events.Count + _keyedEvents.Count + _buckets.Count);

        result.AddRange(_events);
        result.AddRange(_keyedEvents.Values);

        foreach (var bucket in _buckets.Values)
            if (bucket.Count > 0)
                result.Add(bucket.BuildNotification());

        _buckets.Clear();
        _keyedEvents.Clear();
        _events.Clear();

        return result;
    }


    private void AddBatchable(IBatchableDomainEvent @event)
    {
        var type = @event.GetType();

        if (!_buckets.TryGetValue(type, out var bucket))
        {
            bucket = CreateBucket(type);
            _buckets[type] = bucket;
        }

        bucket.Add(@event);
    }

    private static IBatchBucket CreateBucket(Type eventType)
    {
        var factory = BucketFactories.GetOrAdd(eventType, static type =>
        {
            var bucketType = typeof(BatchBucket<>).MakeGenericType(type);
            var constructor = Expression.New(bucketType);
            var convert = Expression.Convert(constructor, typeof(IBatchBucket));

            return Expression.Lambda<Func<IBatchBucket>>(convert).Compile();
        });

        return factory();
    }

    private sealed class CollectionScope(DomainEventScope scope) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            scope._collectionDepth--;
            _disposed = true;
        }
    }
}
