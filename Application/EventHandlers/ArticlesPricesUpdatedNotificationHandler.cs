using Application.Events;
using Application.Handlers.Prices.RecalculateUsablePrice;
using MediatR;

namespace Application.EventHandlers;

public class ArticlesPricesUpdatedNotificationHandler(IMediator mediator) : INotificationHandler<ArticlePricesUpdatedEvent>
{
    public async Task Handle(ArticlePricesUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var command = new RecalculateUsablePriceCommand(notification.ArticleIds);
        await mediator.Send(command, cancellationToken);
    }
}