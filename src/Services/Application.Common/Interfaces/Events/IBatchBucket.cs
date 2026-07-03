using Domain.Interfaces.Events;

namespace Application.Common.Interfaces.Events;

public interface IBatchBucket
{
    void Add(IBatchableDomainEvent item);
    int Count { get; }
    IDomainEvent BuildNotification();
}
