using Application.Events;
using Core.Interfaces;
using MediatR;

namespace Application.EventHandlers;

public class MarkupRangesUpdatedNotificationHandler(IMessageBroker messageBroker, IPriceSetup priceSetup)
    : INotificationHandler<MarkupRangesUpdatedEvent>
{
    public async Task Handle(MarkupRangesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(new Core.Contracts.MarkupRangesUpdatedEvent(), cancellationToken);
        await priceSetup.SetupAsync(cancellationToken);
    }
}