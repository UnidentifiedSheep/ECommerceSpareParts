using Application.Common.Interfaces;
using Core.Interfaces;
using Core.Models;
using Exceptions.Exceptions.Currencies;
using Main.Abstractions.Interfaces.Pricing;
using Main.Abstractions.Interfaces.Services;
using Main.Application.Handlers.Users.GetUserDiscount;
using MediatR;

namespace Main.Application.Handlers.Prices.GetDetailedPrices;

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

    private async Task<decimal?> GetUserDiscount(Guid? buyerId, CancellationToken cancellationToken)
    { 
        if (buyerId == null) return null;
        var query = new GetUserDiscountQuery(buyerId.Value);
        return (await mediator.Send(query, cancellationToken)).Discount;
    }
}