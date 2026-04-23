using Abstractions.Interfaces.Currency;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Enums;
using Main.Abstractions.Exceptions.Articles;
using Main.Abstractions.Exceptions.Logistics;
using Main.Abstractions.Exceptions.Storages;
using Main.Abstractions.Interfaces.Logistics;
using Main.Abstractions.Models.Logistics;
using Main.Application.Dtos.Amw.Logistics;
using Main.Application.Dtos.Storage;
using Main.Application.Interfaces.Persistence;
using Main.Entities;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;
using Mapster;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public record CalculateDeliveryCostQuery(
    string StorageFrom,
    string StorageTo,
    IEnumerable<LogisticsItemDto> Items,
    LogisticsCalculationMode Mode = LogisticsCalculationMode.Strict) : IQuery<CalculateDeliveryCostResult>;

public record CalculateDeliveryCostResult(StorageRouteDto Route, DeliveryCostDto DeliveryCost);

public class CalculateDeliveryCostHandler(
    ILogisticsCostService logisticsCostService,
    IRepository<ProductSize, int> sizesRepository,
    IStorageRouteRepository storageRoutesRepository,
    IRepository<ProductWeight, int> weightRepository,
    ICurrencyConverter currencyConverter)
    : IQueryHandler<CalculateDeliveryCostQuery, CalculateDeliveryCostResult>
{
    public async Task<CalculateDeliveryCostResult> Handle(
        CalculateDeliveryCostQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.StorageFrom;
        var to = request.StorageTo;

        var route = await GetStorageRoute(from, to, cancellationToken);
        var usableArticleIds = request.Items
            .Select(x => x.ArticleId)
            .ToHashSet();

        var sizes = await GetSizes(usableArticleIds, cancellationToken);
        var weights = await GetWeights(usableArticleIds, cancellationToken);

        var deliveryCost = GetDeliveryCost(route, sizes, weights, request.Items, route.CurrencyId, request.Mode)
            .Adapt<DeliveryCostDto>();
        deliveryCost.CurrencyId = route.CurrencyId;

        return new CalculateDeliveryCostResult(route.Adapt<StorageRouteDto>(), deliveryCost);
    }

    private async Task<StorageRoute> GetStorageRoute(string from, string to, CancellationToken cancellationToken)
    {
        var criteria = Criteria<StorageRoute>.New()
            .Include(x => x.Currency)
            .Track(false)
            .Build();
        
        return await storageRoutesRepository.GetActiveRouteAsync(from, to, criteria, cancellationToken)
               ?? throw new StorageRouteNotFound(from, to);
    }

    private async Task<Dictionary<int, ProductId>> GetSizes(
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken)
    {
        return (await sizesRepository
                .GetArticleSizesByIds(articleIds, false, cancellationToken))
            .ToDictionary(x => x.ArticleId);
    }

    private async Task<Dictionary<int, ProductWeight>> GetWeights(
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken)
    {
        return (await weightRepository
                .GetArticleWeightsByIds(articleIds, false, cancellationToken))
            .ToDictionary(x => x.ProductId);
    }

    private LogisticsCalcResult GetDeliveryCost(
        StorageRoute route,
        Dictionary<int, ProductId> sizes,
        Dictionary<int, ProductWeight> weights,
        IEnumerable<LogisticsItemDto> items,
        int currencyId,
        LogisticsCalculationMode mode)
    {
        var priceKg = Math.Round(currencyConverter.ConvertTo(route.PriceKg, route.CurrencyId, currencyId), 2);
        var priceArea = Math.Round(currencyConverter.ConvertTo(route.PricePerM3, route.CurrencyId, currencyId), 2);
        var priceOrder = Math.Round(currencyConverter.ConvertTo(route.PricePerOrder, route.CurrencyId, currencyId), 2);
        var minimalPrice =
            Math.Round(currencyConverter.ConvertTo(route.MinimumPrice ?? 0, route.CurrencyId, currencyId), 2);

        var context = new LogisticsContext(priceKg, priceArea, priceOrder, minimalPrice);
        List<LogisticsItem> logisticsItems = [];

        foreach (var item in items)
        {
            var sizeExists = sizes.TryGetValue(item.ArticleId, out var size);
            var weightExists = weights.TryGetValue(item.ArticleId, out var weight);

            if (mode == LogisticsCalculationMode.Strict)
            {
                if (!sizeExists) throw new ProductSizesNotFoundException(item.ArticleId);
                if (!weightExists) throw new ProductWeightNotFoundException(item.ArticleId);
            }

            logisticsItems.Add(new LogisticsItem(item.ArticleId, item.Quantity, weight?.Weight ?? 0,
                weight?.Unit ?? WeightUnit.Kilogram, size?.VolumeM3 ?? 0));
        }

        if (logisticsItems.Count == 0) throw new NoLogisticsItemsException();

        return logisticsCostService.Calculate(route.PricingModel, context, logisticsItems);
    }
}