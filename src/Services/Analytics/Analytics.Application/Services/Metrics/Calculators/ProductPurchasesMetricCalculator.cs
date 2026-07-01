using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Entities.Metrics.JsonDataModels;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Services.Metrics.Calculators;

public class ProductPurchasesMetricCalculator(
    IRepository<PurchasesFact, Guid> repository,
    ICurrencyConverter currencyConverter
) : MetricCalculatorBase<ProductPurchasesMetric>
{
    private const int BatchSize = 1000;

    public override async Task CalculateMetric(
        ProductPurchasesMetric metric,
        CancellationToken cancellationToken = default)
    {
        var minPrice = decimal.MaxValue;
        var maxPrice = decimal.MinValue;
        decimal totalAmount = 0;
        var totalQuantity = 0;

        double mean = 0;
        double m2 = 0;
        long count = 0;


        var (start, end) = await WithTimer(async () =>
        {
            var lastId = Guid.Empty;
            while (true)
            {
                var facts = await repository.ListAsync(
                    GetCriteria(metric, lastId),
                    cancellationToken);
                if (facts.Count == 0) break;

                foreach (var fact in facts)
                foreach (var item in fact.PurchaseContents)
                {
                    if (item.ProductId != metric.ProductId) continue;

                    var priceDecimal = await currencyConverter.ConvertToBaseAsync(
                        item.Price,
                        fact.CurrencyId,
                        cancellationToken);
                    var quantity = item.Count;

                    if (quantity <= 0) continue;

                    var price = (double)priceDecimal;

                    if (priceDecimal < minPrice) minPrice = priceDecimal;
                    if (priceDecimal > maxPrice) maxPrice = priceDecimal;

                    totalAmount += priceDecimal * quantity;
                    totalQuantity += quantity;

                    count += quantity;

                    var delta = price - mean;
                    mean += delta * quantity / count;
                    var delta2 = price - mean;
                    m2 += quantity * delta * delta2;
                }

                if (facts.Count < BatchSize) break;

                lastId = facts[^1].Id;
            }
        });

        var variance = count > 1 ? m2 / count : 0.0;
        var volatility = (decimal)Math.Sqrt(variance);

        var avgPrice = totalQuantity == 0 ? 0 : totalAmount / totalQuantity;

        var data = new ProductInfoModel
        {
            Quantity = totalQuantity,
            TotalAmount = totalAmount,
            PriceInfo = new PriceInfoModel
            {
                AveragePrice = avgPrice,
                MaximumPrice = totalQuantity == 0 ? 0 : maxPrice,
                MinimumPrice = totalQuantity == 0 ? 0 : minPrice,
                Volatility = volatility
            },
            Timer = new MetricTimer(start, end)
        };

        metric.SetData(data);
        metric.CompleteRecalculation();
    }

    private static Criteria<PurchasesFact> GetCriteria(ProductPurchasesMetric metric, Guid lastId)
    {
        return Criteria<PurchasesFact>.New()
            .Where(x => x.PurchaseContents.Any(z => z.ProductId == metric.ProductId)
                        && metric.RangeStart <= x.CreatedAt && x.CreatedAt <= metric.RangeEnd
                        && x.Id > lastId)
            .Include(x => x.PurchaseContents)
            .OrderByAsc(x => x.Id)
            .Size(BatchSize)
            .Build();
    }
}