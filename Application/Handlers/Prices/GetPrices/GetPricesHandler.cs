using Application.Handlers.Users.GetUserDiscount;
using Application.Interfaces;
using Core.Exceptions.Currencies;
using Core.Interfaces;
using Core.Interfaces.Services;
using MediatR;

namespace Application.Handlers.Prices.GetPrices;

public record GetPricesQuery(IEnumerable<int> ArticleIds, int CurrencyId, string? BuyerId) : IQuery<GetPricesResult>;
public record GetPricesResult(Dictionary<int, double> Prices);

public class GetPricesHandler(ICurrencyConverter currencyConverter, IArticlePricesService pricesService, 
    IPriceGenerator priceGenerator, IMediator mediator) : IQueryHandler<GetPricesQuery, GetPricesResult>
{
    public async Task<GetPricesResult> Handle(GetPricesQuery request, CancellationToken cancellationToken)
    {
        ValidateData(request.CurrencyId);
        
        var currencyId = request.CurrencyId;
        var buyerId = request.BuyerId;
        var articleIds = request.ArticleIds;
        
        var results = new Dictionary<int, double>();
        var userDiscount = await GetUserDiscount(buyerId, cancellationToken) ?? 0;
        var prices = await pricesService.GetUsablePricesAsync(articleIds, cancellationToken);
        
        foreach (var (articleId, usablePrice) in prices)
        {
            if (usablePrice == null || usablePrice <= 0) continue;
            var converted = currencyConverter.ConvertFromUsd(usablePrice.Value, currencyId);
            var sellPrice = priceGenerator.GetSellPrice(converted, (double)userDiscount, currencyId);
            results[articleId] = sellPrice;
        }

        return new GetPricesResult(results);
    }
    
    private async Task<decimal?> GetUserDiscount(string? buyerId, CancellationToken cancellationToken)
    {
        if (buyerId == null) return null;
        var query = new GetUserDiscountQuery(buyerId);
        return (await mediator.Send(query, cancellationToken)).Discount;
    }

    private void ValidateData(int currencyId)
    {
        if(!currencyConverter.IsSupportedCurrency(currencyId))
            throw new CurrencyNotFoundException(currencyId);
    }
}