using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using MediatR;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models;

namespace Pricing.Application.Handlers.Prices.GetDetailedPrices;

public record GetDetailedPricesQuery(IEnumerable<int> ArticleIds, int CurrencyId, Guid? BuyerId)
    : IQuery<GetDetailedPriceResult>;

public record GetDetailedPriceResult(Dictionary<int, DetailedPriceModel> Prices);

public class GetDetailedPriceHandler(ICurrencyConverter currencyConverter, IBasePricesService pricesService,
    IMarkupService markupService, IMediator mediator) : IQueryHandler<GetDetailedPricesQuery, GetDetailedPriceResult>
{
    public async Task<GetDetailedPriceResult> Handle(GetDetailedPricesQuery request, CancellationToken cancellationToken)
    {
        /*var buyerId = request.BuyerId;
        var currencyId = request.CurrencyId;
        var articleIds = request.ArticleIds;

        var results = new Dictionary<int, DetailedPriceModel>();
        var userDiscount = await GetUserDiscount(buyerId, cancellationToken) ?? 0;
        var prices = await pricesService.GetUsablePricesAsync(articleIds, cancellationToken);

        foreach (var (articleId, usablePrice) in prices)
        {
            if (usablePrice is null or <= 0) continue;
            var converted = currencyConverter.ConvertFromUsd(usablePrice.Value, currencyId);
            results[articleId] = new DetailedPriceModel
            {
                Id = articleId,
                PriceInUsd = usablePrice.Value,
                MinPrice = markupService.GetSellPriceWithMinimalMarkUp(converted),
                RecommendedPrice = markupService.GetSellPrice(converted, 0, currencyId),
                RecommendedPriceWithDiscount = markupService.GetSellPrice(converted, (double)userDiscount, currencyId)
            };
        }

        return new GetDetailedPriceResult(results);*/
        throw new NotImplementedException();
    }
}