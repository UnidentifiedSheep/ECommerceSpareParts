using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using MediatR;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Application.Handlers.Prices.RecalculateBasePrices;

namespace Pricing.Application.Handlers.Prices.GetBasePrices;

public record GetBasePricesQuery(IEnumerable<int> ArticleIds, int CurrencyId) : IQuery<GetBasePricesResult>;
public record GetBasePricesResult(Dictionary<int, BasePricingItemResult?> Prices);

public class GetBasePricesHandler(IArticlePricesCacheRepository articlePricesCache, ICurrencyConverter currencyConverter,
    IMediator mediator) 
    : IQueryHandler<GetBasePricesQuery, GetBasePricesResult>
{
    public async Task<GetBasePricesResult> Handle(GetBasePricesQuery request, CancellationToken cancellationToken)
    {
        var foundPrices = await articlePricesCache.GetArticleBasePrices(request.ArticleIds);
        HashSet<int> notFoundIds = [];
        Dictionary<int, BasePricingItemResult?> result = new();
        IterateOverPrices(foundPrices, result, notFoundIds, request.CurrencyId);
        await mediator.Send(new RecalculateBasePricesCommand(notFoundIds), cancellationToken);
        
        foundPrices = await articlePricesCache.GetArticleBasePrices(notFoundIds);
        notFoundIds.Clear();
        IterateOverPrices(foundPrices, result, notFoundIds, request.CurrencyId);
        return new GetBasePricesResult(result);
    }

    private void IterateOverPrices(Dictionary<int, BasePricingItemResult?> prices, 
        Dictionary<int, BasePricingItemResult?> result, 
        HashSet<int> notFoundIds, int currencyId)
    {
        foreach (var (articleId, basePrice) in prices)
        {
            if (basePrice == null)
            {
                notFoundIds.Add(articleId);
                result[articleId] = null;
                continue;
            }
            
            result[articleId] = basePrice;
            result[articleId] = ConvertPrice(basePrice, currencyId);
        }
    }

    private BasePricingItemResult ConvertPrice(BasePricingItemResult pricingItem, int targetCurrencyId)
    {
        decimal basePrice = currencyConverter.ConvertFromUsd(pricingItem.BasePrice, targetCurrencyId);
        decimal finalPrice = currencyConverter.ConvertFromUsd(pricingItem.FinalPrice, targetCurrencyId);
        return pricingItem with { BasePrice = basePrice, FinalPrice = finalPrice };
    }
}