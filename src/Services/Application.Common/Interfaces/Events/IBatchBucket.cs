using Domain.Interfaces.Events;

namespace Application.Common.Interfaces.Events;

public interface IBatchBucket
{
	int Count { get; }

	void Add(IBatchableDomainEvent item);

	IDomainEvent BuildNotification();
}
