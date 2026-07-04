using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Entities.Metrics.JsonDataModels;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Services.Metrics.Calculators;

public class ProductSalesMetricCalculator(
    IRepository<SalesFact, Guid> salesRepository
)
    : MetricCalculatorBase<ProductSalesMetric>
{
    private const int BatchSize = 1000;

    public override async Task CalculateMetric(
        ProductSalesMetric metric,
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
                var facts = await salesRepository.ListAsync(
                    GetCriteria(metric, lastId),
                    cancellationToken);
                if (facts.Count == 0) break;

                foreach (var item in facts.SelectMany(x => x.SaleContents))
                {
                    if (item.ProductId != metric.ProductId) continue;

                    var priceDecimal = item.PriceInBaseCurrency;
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

    private static Criteria<SalesFact> GetCriteria(ProductSalesMetric metric, Guid lastId)
    {
        return Criteria<SalesFact>.New()
            .Where(x => x.SaleContents.Any(z => z.ProductId == metric.ProductId)
                        && metric.RangeStart <= x.CreatedAt && x.CreatedAt <= metric.RangeEnd
                        && x.Id > lastId)
            .Include(x => x.SaleContents)
            .OrderByAsc(x => x.Id)
            .Size(BatchSize)
            .Build();
    }
}