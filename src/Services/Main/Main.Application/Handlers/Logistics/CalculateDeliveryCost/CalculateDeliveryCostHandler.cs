using Application.Common.Interfaces;
using Core.Interfaces;
using Exceptions.Exceptions.ArticleSizes;
using Exceptions.Exceptions.ArticleWeight;
using Exceptions.Exceptions.Logistics;
using Exceptions.Exceptions.StorageRoutes;
using Main.Abstractions.Dtos.Amw.Logistics;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models.Logistics;
using Main.Entities;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public record CalculateDeliveryCostQuery(string StorageFrom, string StorageTo,
    IEnumerable<LogisticsItemDto> Items, LogisticsCalculationMode Mode = LogisticsCalculationMode.Strict) : IQuery<CalculateDeliveryCostResult>;
public record CalculateDeliveryCostResult(StorageRouteDto Route, DeliveryCostDto DeliveryCost);

public class CalculateDeliveryCostHandler(ILogisticsCostService logisticsCostService, 
    IArticleSizesRepository sizesRepository, IStorageRoutesRepository storageRoutesRepository,
    IArticleWeightRepository weightRepository, ICurrencyConverter currencyConverter) 
    : IQueryHandler<CalculateDeliveryCostQuery, CalculateDeliveryCostResult>
{
    public async Task<CalculateDeliveryCostResult> Handle(CalculateDeliveryCostQuery request, CancellationToken cancellationToken)
    {
        string from = request.StorageFrom;
        string to = request.StorageTo;
        
        var route = await GetStorageRoute(from, to, cancellationToken);
        var usableArticleIds = request.Items
            .Select(x => x.ArticleId)
            .ToHashSet();
        
        Dictionary<int, ArticleSize> sizes = await GetSizes(usableArticleIds, cancellationToken);
        Dictionary<int, Entities.ArticleWeight> weights = await GetWeights(usableArticleIds, cancellationToken);
        
        var deliveryCost = GetDeliveryCost(route, sizes, weights, request.Items, route.CurrencyId, request.Mode)
            .Adapt<DeliveryCostDto>();
        deliveryCost.CurrencyId = route.CurrencyId;
        
        return new CalculateDeliveryCostResult(route.Adapt<StorageRouteDto>(), deliveryCost);
    }

    private async Task<StorageRoute> GetStorageRoute(string from, string to, CancellationToken cancellationToken)
    {
        return await storageRoutesRepository.GetStorageRouteAsync(from, to, true, false, 
                cancellationToken, x => x.Currency) 
            ?? throw new StorageRouteNotFound(from, to);
    }
    
    private async Task<Dictionary<int, ArticleSize>> GetSizes(IEnumerable<int> articleIds, CancellationToken cancellationToken)
    {
        return (await sizesRepository
                .GetArticleSizesByIds(articleIds, false, cancellationToken))
            .ToDictionary(x => x.ArticleId);
    }
    
    private async Task<Dictionary<int, Entities.ArticleWeight>> GetWeights(IEnumerable<int> articleIds, CancellationToken cancellationToken)
    {
        return (await weightRepository
                .GetArticleWeightsByIds(articleIds, false, cancellationToken))
            .ToDictionary(x => x.ArticleId);
    }

    private LogisticsCalcResult GetDeliveryCost(StorageRoute route, Dictionary<int, ArticleSize> sizes,
        Dictionary<int, Entities.ArticleWeight> weights, IEnumerable<LogisticsItemDto> items, int currencyId, 
        LogisticsCalculationMode mode)
    {
        decimal priceKg = Math.Round(currencyConverter.ConvertTo(route.PriceKg, route.CurrencyId, currencyId), 2);
        decimal priceArea = Math.Round(currencyConverter.ConvertTo(route.PricePerM3, route.CurrencyId, currencyId), 2);
        decimal priceOrder = Math.Round(currencyConverter.ConvertTo(route.PricePerOrder, route.CurrencyId, currencyId), 2);
        decimal minimalPrice = Math.Round(currencyConverter.ConvertTo(route.MinimumPrice ?? 0, route.CurrencyId, currencyId), 2);
        
        LogisticsContext context = new LogisticsContext(priceKg, priceArea, priceOrder, minimalPrice);
        List<LogisticsItem> logisticsItems = [];

        foreach (var item in items)
        {
            var sizeExists = sizes.TryGetValue(item.ArticleId, out var size);
            var weightExists = weights.TryGetValue(item.ArticleId, out var weight);

            if (mode == LogisticsCalculationMode.Strict)
            {
                if (!sizeExists) throw new ArticleSizesNotFoundException(item.ArticleId);
                if (!weightExists) throw new ArticleWeightNotFound(item.ArticleId);
            }
            logisticsItems.Add(new LogisticsItem(item.ArticleId, item.Quantity, weight?.Weight ?? 0, 
                weight?.Unit ?? WeightUnit.Kilogram, size?.VolumeM3 ?? 0));
        }

        if (logisticsItems.Count == 0) throw new NoLogisticsItemsException();

        return logisticsCostService.Calculate(route.PricingModel, context, logisticsItems);
    }
}