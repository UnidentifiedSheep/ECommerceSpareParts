using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Settings;
using Contracts.ArticleCoefficients.GetArticleCoefficients;
using Contracts.Models.StorageContent;
using Contracts.StorageContent.GetStorageContentCosts;
using Mapster;
using MassTransit;
using MediatR;
using Pricing.Abstractions.Interfaces.CacheRepositories;
using Pricing.Abstractions.Interfaces.Services;
using Pricing.Abstractions.Models.Pricing;
using Pricing.Enums;

namespace Pricing.Application.Handlers.Prices.RecalculateBasePrices;

public record RecalculateBasePricesCommand(IEnumerable<int> ArticleIds) : ICommand;
/// <summary>
/// All prices are in USD
/// </summary>
/// <param name="Prices">Calculated prices</param>
public record RecalculateBasePricesResult(List<BasePricingItemResult> Prices);

public class RecalculateBasePricesHandler(IArticlePricesCacheRepository articlePricesCache,
    IRequestClient<GetStorageContentCostsRequest> costsRequestClient,
    IRequestClient<GetArticleCoefficientsRequest> coefficientsRequestClient,
    ICurrencyConverter currencyConverter, IBasePricesService basePricesService,
    ISettingsContainer settingsContainer)
    : ICommandHandler<RecalculateBasePricesCommand>
{
    public async Task<Unit> Handle(RecalculateBasePricesCommand request, CancellationToken cancellationToken)
    {
        var ids = request.ArticleIds.ToHashSet();
        var pricingType = GetPricingType();
        var pricesResponse = await costsRequestClient
            .GetResponse<GetStorageContentCostsResponse>(new GetStorageContentCostsRequest { ArticleIds = ids}, cancellationToken);
        
        var buyPrices = pricesResponse.Message.StorageContentCosts.GroupBy(x => x.ArticleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var coefficients = (await coefficientsRequestClient
            .GetResponse<GetArticleCoefficientsResponse>(ids, cancellationToken)).Message.Coefficients;
        
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
                .Select(ContentCostToPrice)
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

    private ArticlePrice ContentCostToPrice(StorageContentCost contentCost)
    {
        decimal buyPrice = currencyConverter.ConvertToUsd(contentCost.Price, contentCost.CurrencyId);
        decimal deliveryPrice = currencyConverter.ConvertToUsd(contentCost.DeliveryPrice, contentCost.DeliveryCurrencyId);
        return new ArticlePrice(buyPrice, deliveryPrice);
    }

    private ArticlePricingType GetPricingType()
    {
        var setting = settingsContainer.GetSetting(Abstractions.Constants.Settings.Pricing);
        return setting.PricingStrategy;
    }
}