using Contracts.Markup;
using Core.Interfaces;
using Main.Application.Notifications;
using Main.Core.Interfaces.Pricing;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class MarkupRangesUpdatedNotificationHandler(IMessageBroker messageBroker, IPriceSetup priceSetup)
    : INotificationHandler<MarkupRangesUpdatedNotification>
{
    public async Task Handle(MarkupRangesUpdatedNotification notification, CancellationToken cancellationToken)
    {
        await messageBroker.Publish(new MarkupRangesUpdatedEvent(), cancellationToken);
        await priceSetup.SetupAsync(cancellationToken);
    }
}