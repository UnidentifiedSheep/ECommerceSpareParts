using Domain.Interfaces.Events;
using MediatR;

namespace Application.Common.Interfaces.Events;

public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IDomainEvent
{
}