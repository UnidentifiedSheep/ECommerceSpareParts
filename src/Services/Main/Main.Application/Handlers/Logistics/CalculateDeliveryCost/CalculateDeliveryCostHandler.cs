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
using Main.Application.Handlers.Projections;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Application.Handlers.Logistics.CalculateDeliveryCost;

public record CalculateDeliveryCostQuery(
    string StorageFrom,
    string StorageTo,
    IEnumerable<LogisticsItemDto> Items,
    LogisticsCalculationMode Mode = LogisticsCalculationMode.Strict
    ) : IQuery<CalculateDeliveryCostResult>;

public record CalculateDeliveryCostResult(StorageRouteDto Route, DeliveryCostDto DeliveryCost);

public class CalculateDeliveryCostHandler(
    ILogisticsCostService logisticsCostService,
    IRepository<ProductSize, int> sizesRepository,
    IStorageRouteRepository storageRoutesRepository,
    IRepository<Entities.Product.ProductWeight, int> weightRepository,
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
        var usableProductIds = request.Items
            .Select(x => x.ProductId)
            .ToHashSet();

        var sizes = await GetSizes(usableProductIds, cancellationToken);
        var weights = await GetWeights(usableProductIds, cancellationToken);

        var calcResult = CalculateLogistics(route, sizes, weights, request.Items, route.CurrencyId, request.Mode);
        DeliveryCostDto deliveryCost = new()
        {
            TotalAreaM3 = calcResult.TotalAreaM3,
            TotalWeight = calcResult.TotalWeight,
            TotalCost = calcResult.TotalCost,
            CurrencyId = route.CurrencyId,
            Items = calcResult.Items.Select(x => new DeliveryCostItemDto
            {
                AreaM3 = x.AreaM3,
                AreaPerItem = x.AreaPerItem,
                Cost = x.Cost,
                ProductId = x.Id,
                Quantity = x.Quantity,
                Reasons = x.Reasons,
                Skipped = x.Skipped,
                Weight = x.Weight,
                WeightPerItem = x.WeightPerItem,
                WeightUnit = x.WeightUnit
            }).ToList(),
            MinimalPrice = calcResult.MinimalPrice,
            MinimalPriceApplied = calcResult.MinimalPriceApplied,
            PricingModel = calcResult.PricingModel,
            WeightUnit = calcResult.WeightUnit
        };

        return new CalculateDeliveryCostResult(StorageProjections.StorageRouteProjectionFunc(route), deliveryCost);
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

    private async Task<Dictionary<int, ProductSize>> GetSizes(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<ProductSize>.New()
            .Where(x => productIds.Contains(x.ProductId))
            .Track(false)
            .Build();
            
        return (await sizesRepository
                .ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.ProductId);
    }

    private async Task<Dictionary<int, Entities.Product.ProductWeight>> GetWeights(
        IEnumerable<int> productIds,
        CancellationToken cancellationToken)
    {
        var criteria = Criteria<Entities.Product.ProductWeight>.New()
            .Where(x => productIds.Contains(x.ProductId))
            .Track(false)
            .Build();
        return (await weightRepository.ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.ProductId);
    }

    private LogisticsCalcResult CalculateLogistics(
        StorageRoute route,
        Dictionary<int, ProductSize> sizes,
        Dictionary<int, Entities.Product.ProductWeight> weights,
        IEnumerable<LogisticsItemDto> items,
        int currencyId,
        LogisticsCalculationMode mode)
    {
        var priceKg = Math.Round(currencyConverter.ConvertTo(route.PriceKg, route.CurrencyId, currencyId), 2);
        var priceArea = Math.Round(currencyConverter.ConvertTo(route.PricePerM3, route.CurrencyId, currencyId), 2);
        var priceOrder = Math.Round(currencyConverter.ConvertTo(route.PricePerOrder, route.CurrencyId, currencyId), 2);
        var minimalPrice = Math.Round(currencyConverter.ConvertTo(route.MinimumPrice, route.CurrencyId, currencyId), 2);

        var context = new LogisticsContext(priceKg, priceArea, priceOrder, minimalPrice);
        List<LogisticsItem> logisticsItems = [];

        foreach (var item in items)
        {
            var sizeExists = sizes.TryGetValue(item.ProductId, out var size);
            var weightExists = weights.TryGetValue(item.ProductId, out var weight);

            if (mode == LogisticsCalculationMode.Strict)
            {
                if (!sizeExists) throw new ProductSizesNotFoundException(item.ProductId);
                if (!weightExists) throw new ProductWeightNotFoundException(item.ProductId);
            }

            logisticsItems.Add(new LogisticsItem(item.ProductId, item.Quantity, weight?.Weight ?? 0,
                weight?.Unit ?? WeightUnit.Kilogram, size?.VolumeM3 ?? 0));
        }

        if (logisticsItems.Count == 0) throw new NoLogisticsItemsException();

        return logisticsCostService.Calculate(route.PricingModel, context, logisticsItems);
    }
}