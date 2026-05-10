using Analytics.Application.Interfaces.Repositories;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;
using Application.Common.Interfaces.Currency;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Services.Metrics.Calculators;

public class ArticleSalesMetricCalculator(
    ISalesFactRepository salesRepository,
    ICurrencyConverter currencyConverter)
    : MetricCalculatorBase<ArticleSalesMetric>
{
    public override async Task CalculateMetric(ArticleSalesMetric metric, CancellationToken cancellationToken = default)
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
            await foreach (var fact in salesRepository
                               .AsyncEnumerable(GetCriteria(metric))
                               .WithCancellation(cancellationToken))
            foreach (var item in fact.SaleContents)
            {
                if (item.ArticleId != metric.ArticleId)
                    continue;

                var priceDecimal = await currencyConverter.ConvertToBaseAsync(
                    item.Price, 
                    fact.CurrencyId, 
                    cancellationToken);
                var quantity = item.Count;

                if (quantity <= 0)
                    continue;

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
        });

        var variance = count > 1 ? m2 / count : 0.0;
        var volatility = (decimal)Math.Sqrt(variance);

        var avgPrice = totalQuantity == 0 ? 0 : totalAmount / totalQuantity;

        var data = new ArticleInfoModel
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

    private static Criteria<SalesFact> GetCriteria(ArticleSalesMetric metric)
    {
        return Criteria<SalesFact>.New()
            .Where(x => x.SaleContents.Any(z => z.ArticleId == metric.ArticleId)
                        && metric.RangeStart <= x.CreatedAt && x.CreatedAt <= metric.RangeEnd)
            .Build();
    }
}
