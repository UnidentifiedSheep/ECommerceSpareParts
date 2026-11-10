using Application.Common.Interfaces.Events;
using Domain.Interfaces.Events;

namespace Application.Common.Services.Events;

public class DomainEventScope : IDomainEventScope
{
	private readonly DomainEventBuffer _beforeSave = new();
	private readonly DomainEventBuffer _afterSave = new();
	private int _collectionDepth;

	public bool IsCollectionEnabled => _collectionDepth > 0;
	public bool HasBeforeSaveEvents => !_beforeSave.IsEmpty;

	public IDisposable EnableCollection()
	{
		_collectionDepth++;
		return new CollectionScope(this);
	}

	public void Add<T>(T @event) where T : IDomainEvent
	{
		ArgumentNullException.ThrowIfNull(@event);
		var buffer = @event is IBeforeSaveDomainEvent ? _beforeSave : _afterSave;
		buffer.Add(@event);
	}

	public void AddRange<T>(IEnumerable<T> events) where T : IDomainEvent
	{
		foreach (var @event in events)
			Add(@event);
	}

	public IReadOnlyCollection<IDomainEvent> Flush() => [.. _beforeSave.Flush(), .. _afterSave.Flush()];

	public IReadOnlyCollection<IDomainEvent> FlushBeforeSave() => _beforeSave.Flush();

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
