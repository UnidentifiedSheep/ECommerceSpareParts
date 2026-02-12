using Contracts.Articles;
using MassTransit;
using MediatR;
using Pricing.Application.Handlers.Prices.RecalculateBasePrices;

namespace Pricing.Application.Consumers;

public class ArticleBuyPricesChangedConsumer(IMediator mediator) : IConsumer<ArticleBuyPricesChangedEvent >
{
    public async Task Consume(ConsumeContext<ArticleBuyPricesChangedEvent> context)
    {
        await mediator.Send(new RecalculateBasePricesCommand(context.Message.ArticleIds));
    }
}