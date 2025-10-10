using Main.Application.Handlers.Prices.RecalculateUsablePrice;
using Main.Application.Notifications;
using MediatR;

namespace Main.Application.NotificationHandlers;

public class ArticlesPricesUpdatedNotificationHandler(IMediator mediator)
    : INotificationHandler<ArticlePricesUpdatedNotification>
{
    public async Task Handle(ArticlePricesUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var command = new RecalculateUsablePriceCommand(notification.ArticleIds);
        await mediator.Send(command, cancellationToken);
    }
}