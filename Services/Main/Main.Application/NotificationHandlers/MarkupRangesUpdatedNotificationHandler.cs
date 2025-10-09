using Core.Interfaces;
using Main.Application.Events;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class MarkupRangesUpdatedNotificationHandler(IMessageBroker messageBroker, IPriceSetup priceSetup)
    : INotificationHandler<MarkupRangesUpdatedEvent>
{
    public async Task Handle(MarkupRangesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(new Core.Contracts.MarkupRangesUpdatedEvent(), cancellationToken);
        await priceSetup.SetupAsync(cancellationToken);
    }
}