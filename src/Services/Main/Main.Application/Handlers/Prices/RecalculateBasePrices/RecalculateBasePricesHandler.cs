using Application.Common.Interfaces;
using Core.Interfaces;
using Main.Abstractions.Interfaces.CacheRepositories;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Services;
using Main.Abstractions.Models;
using Main.Abstractions.Models.Pricing;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Application.Handlers.Prices.RecalculateBasePrices;

public record RecalculateBasePricesCommand(IEnumerable<int> ArticleIds) : ICommand;
/// <summary>
/// All prices are in USD
/// </summary>
/// <param name="Prices">Calculated prices</param>
public record RecalculateBasePricesResult(List<BasePricingItemResult> Prices);

public class RecalculateBasePricesHandler(IArticlePricesCacheRepository articlePricesCache,
    IDefaultSettingsRepository defaultSettingsRepository,
    IStorageContentRepository storageContentRepository, ICurrencyConverter currencyConverter,
    IBasePricesService basePricesService, IArticleCoefficients articleCoefficientsRepository)
    : ICommandHandler<RecalculateBasePricesCommand>
{
    public async Task<Unit> Handle(RecalculateBasePricesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.ArticleIds.ToHashSet();
        var pricingType = await GetPricingType(cancellationToken);
        
        var buyPrices = await storageContentRepository
            .GetStorageContentsForPricing(ids, true, cancellationToken,
                x => x.PurchaseContent);

        var coefficients = await articleCoefficientsRepository
            .GetArticlesCoefficients(ids, false, cancellationToken, 
                x => x.CoefficientNameNavigation);
        
        List<BasePricingItem> data = [];
        List<int> articlesWithQtyZero = [];

        foreach (var (articleId, projections) in buyPrices)
        {
            if (projections.Count == 0)
            {
                articlesWithQtyZero.Add(articleId);
                continue;
            }

            List<PriceCoefficient> coefs = coefficients.TryGetValue(articleId, out var articleCoefficients) 
                ? articleCoefficients.Adapt<List<PriceCoefficient>>() 
                : [];
            

            List<ArticlePrice> prices = projections
                .Select(ProjectionToPrice)
                .ToList();
            
            data.Add(new BasePricingItem(articleId, prices, coefs));
        }
        
        var calcResult = basePricesService.CalculatePrices(new BasePricingContext(data, pricingType));
        List<(BasePricingItemResult value, TimeSpan? exp)> itemsToCache = calcResult.Items
            .Select(x =>
                {
                    TimeSpan? ttl = x.AppliedCoefficients.Any() 
                        ? x.AppliedCoefficients.Min(z => z.ValidTill - DateTime.UtcNow)
                        : null;
                    if (ttl != null)
                        ttl = TimeSpan.Compare(ttl.Value, TimeSpan.Zero) > 0 ? ttl : TimeSpan.Zero;

                    return (x, ttl);
                }
            ).ToList();
        
        articlePricesCache.SetArticleBasePrices(itemsToCache);
        await articlePricesCache.DeleteArticleBasePrices(articlesWithQtyZero);
        
        return Unit.Value;
    }

    private ArticlePrice ProjectionToPrice(StorageContentLogisticsProjection projection)
    {
        decimal buyPrice = currencyConverter.ConvertToUsd(projection.Price, projection.CurrencyId);
        if (projection.LogisticsCurrencyId == null || projection.LogisticsPrice == null) 
            return new ArticlePrice(buyPrice, 0);
        decimal deliveryPrice = currencyConverter.ConvertToUsd(projection.LogisticsPrice.Value, projection.LogisticsCurrencyId.Value);
        return new ArticlePrice(buyPrice, deliveryPrice);
    }

    private async Task<ArticlePricingType> GetPricingType(CancellationToken cancellationToken)
    {
        var settings = await defaultSettingsRepository.GetDefaultSettingsAsync(cancellationToken);
        return settings.PriceGenerationStrategy;
    }
}