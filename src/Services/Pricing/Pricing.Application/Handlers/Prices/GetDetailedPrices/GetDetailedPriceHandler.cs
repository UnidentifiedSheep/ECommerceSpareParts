using Abstractions.Models;
using Application.Common.Interfaces;
using MediatR;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Interfaces.Services.Pricing;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Application.Handlers.Prices.GetBasePrices;

namespace Pricing.Application.Handlers.Prices.GetDetailedPrices;

public record GetDetailedPricesQuery(IEnumerable<int> ArticleIds, int CurrencyId, Guid? BuyerId)
    : IQuery<GetDetailedPriceResult>;

/// <summary>
/// Key - articleId, Value - detailed price model
/// </summary>
public record GetDetailedPriceResult(Dictionary<int, PricingResult?> Prices);

public class GetDetailedPriceHandler(IMediator mediator, IPriceService priceService, IUserCacheRepository userCacheRepository) 
    : IQueryHandler<GetDetailedPricesQuery, GetDetailedPriceResult>
{
    public async Task<GetDetailedPriceResult> Handle(GetDetailedPricesQuery request, CancellationToken cancellationToken)
    {
        var buyerId = request.BuyerId;
        var currencyId = request.CurrencyId;
        var articleIds = request.ArticleIds;

        var results = new Dictionary<int, PricingResult?>();
        var userDiscount = await GetUserDiscount(buyerId);
        var prices = await GetBasePrices(articleIds, request.CurrencyId, cancellationToken);

        foreach (var (articleId, basePrice) in prices)
        {
            if (basePrice == null)
            {
                results[articleId] = null;
                continue;
            }

            results[articleId] =
                priceService.GetPrice(new PricingContext(basePrice.FinalPrice, userDiscount, currencyId));
        }

        return new GetDetailedPriceResult(results);
    }

    private async Task<decimal> GetUserDiscount(Guid? userId)
    {
        if (userId == null) return 0;
        Timestamped<decimal>? discount = await userCacheRepository.GetUserDiscount(userId.Value);
        return discount?.Value ?? 0;
    }

    private async Task<Dictionary<int, BasePricingItemResult?>> GetBasePrices(IEnumerable<int> articleIds,
        int currencyId, CancellationToken cancellationToken)
    {
        var query = new GetBasePricesQuery(articleIds, currencyId);
        var result = await mediator.Send(query, cancellationToken);
        return result.Prices;
    }
}