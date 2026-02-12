using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using MediatR;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Interfaces.Services.Pricing;

namespace Pricing.Application.Handlers.Prices.GetPrices;

public record GetPricesQuery(IEnumerable<int> ArticleIds, int CurrencyId, Guid? BuyerId) : IQuery<GetPricesResult>;

public record GetPricesResult(Dictionary<int, decimal> Prices);

public class GetPricesHandler(ICurrencyConverter currencyConverter, IBasePricesService pricesService,
    IMarkupService markupService, IMediator mediator) : IQueryHandler<GetPricesQuery, GetPricesResult>
{
    public async Task<GetPricesResult> Handle(GetPricesQuery request, CancellationToken cancellationToken)
    {
        /*var currencyId = request.CurrencyId;
        var buyerId = request.BuyerId;
        var articleIds = request.ArticleIds;

        var results = new Dictionary<int, double>();
        var userDiscount = await GetUserDiscount(buyerId, cancellationToken) ?? 0;
        var prices = await pricesService.GetUsablePricesAsync(articleIds, cancellationToken);

        foreach (var (articleId, usablePrice) in prices)
        {
            if (usablePrice is null or <= 0) continue;
            var converted = currencyConverter.ConvertFromUsd(usablePrice.Value, currencyId);
            var sellPrice = markupService.GetSellPrice(converted, (double)userDiscount, currencyId);
            results[articleId] = sellPrice;
        }

        return new GetPricesResult(results);*/
        throw new NotImplementedException();
    }
}