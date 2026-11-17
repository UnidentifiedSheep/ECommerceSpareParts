using System.Collections.Concurrent;
using System.Linq.Expressions;
using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

internal sealed class DomainEventBuffer
{
	private static readonly ConcurrentDictionary<Type, Func<IBatchBucket>> BucketFactories = new();

	private readonly Dictionary<Type, IBatchBucket> _buckets = [];

	private readonly List<IDomainEvent> _events = [];

	private readonly Dictionary<string, IDomainEvent> _keyedEvents = new();

	public bool IsEmpty => _events.Count == 0 && _keyedEvents.Count == 0 && _buckets.Count == 0;

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


	public IReadOnlyCollection<IDomainEvent> Flush()
	{
		var result = new List<IDomainEvent>(_events.Count + _keyedEvents.Count + _buckets.Count);

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
		var factory = BucketFactories.GetOrAdd(
			eventType,
			static type =>
			{
				var bucketType = typeof(BatchBucket<>).MakeGenericType(type);
				var constructor = Expression.New(bucketType);
				var convert = Expression.Convert(constructor, typeof(IBatchBucket));

				return Expression.Lambda<Func<IBatchBucket>>(convert).Compile();
			});

		return factory();
	}
}
