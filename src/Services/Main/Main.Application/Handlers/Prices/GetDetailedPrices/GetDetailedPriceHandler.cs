using Application.Common.Interfaces;
using Core.Interfaces;
using Core.Models;
using Exceptions.Exceptions.Currencies;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Core.Interfaces.Pricing;
using Main.Core.Interfaces.Services;
using MediatR;

namespace Main.Application.Handlers.Prices.GetDetailedPrices;

public record GetDetailedPricesQuery(IEnumerable<int> ArticleIds, int CurrencyId, Guid? BuyerId)
    : IQuery<GetDetailedPriceResult>;

public record GetDetailedPriceResult(Dictionary<int, DetailedPriceModel> Prices);

public class GetDetailedPriceHandler(
    ICurrencyConverter currencyConverter,
    IArticlePricesService pricesService,
    IPriceGenerator priceGenerator,
    IMediator mediator) : IQueryHandler<GetDetailedPricesQuery, GetDetailedPriceResult>
{
    public async Task<GetDetailedPriceResult> Handle(GetDetailedPricesQuery request,
        CancellationToken cancellationToken)
    {
        ValidateData(request.CurrencyId);

        var buyerId = request.BuyerId;
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
                MinPrice = priceGenerator.GetSellPriceWithMinimalMarkUp(converted),
                RecommendedPrice = priceGenerator.GetSellPrice(converted, 0, currencyId),
                RecommendedPriceWithDiscount = priceGenerator.GetSellPrice(converted, (double)userDiscount, currencyId)
            };
        }

        return new GetDetailedPriceResult(results);
    }

    private async Task<decimal?> GetUserDiscount(Guid? buyerId, CancellationToken cancellationToken)
    {
        if (buyerId == null) return null;
        var query = new GetUserDiscountQuery(buyerId.Value);
        return (await mediator.Send(query, cancellationToken)).Discount;
    }

    private void ValidateData(int currencyId)
    {
        if (!currencyConverter.IsSupportedCurrency(currencyId))
            throw new CurrencyNotFoundException(currencyId);
    }
}