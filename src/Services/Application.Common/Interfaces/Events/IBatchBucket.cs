using Domain.Interfaces.Events;

namespace Application.Common.Interfaces.Events;

public interface IBatchBucket<in TEvent> : IBatchBucket where TEvent : IBatchableDomainEvent
{
    void Add(TEvent item);
}

public interface IBatchBucket
{
    int Count { get; }
    IDomainEvent BuildNotification();
}