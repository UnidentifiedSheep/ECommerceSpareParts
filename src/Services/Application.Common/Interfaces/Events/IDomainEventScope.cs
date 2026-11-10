using Domain.Interfaces.Events;

namespace Application.Common.Interfaces.Events;

public interface IDomainEventScope
{
	bool IsCollectionEnabled { get; }

	bool HasBeforeSaveEvents { get; }

	IDisposable EnableCollection();

	void Add<T>(T @event) where T : IDomainEvent;

	void AddRange<T>(IEnumerable<T> events) where T : IDomainEvent;

	IReadOnlyCollection<IDomainEvent> Flush();

	/// <summary>Removes only before-save notifications, preserving ordinary events.</summary>
	IReadOnlyCollection<IDomainEvent> FlushBeforeSave();
}
