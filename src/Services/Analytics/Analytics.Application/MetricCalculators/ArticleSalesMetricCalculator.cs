using System.Linq.Expressions;
using Abstractions.Interfaces.Currency;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Entities.Metrics.JsonDataModels;

namespace Analytics.Application.MetricCalculators;

public class ArticleSalesMetricCalculator(
    ISalesRepository salesRepository,
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
            await foreach (var fact in salesRepository.GetFacts(GetWhere(metric)).WithCancellation(cancellationToken))
            {
                var neededArticle = fact.SaleContents
                    .Where(x => x.ArticleId == metric.ArticleId)
                    .Select(x => (price: currencyConverter.ConvertToUsd(x.Price, fact.CurrencyId), quantity: x.Count))
                    .ToList();

                foreach (var (priceDecimal, quantity) in neededArticle)
                {
                    var price = (double)priceDecimal;

                    minPrice = Math.Min(minPrice, priceDecimal);
                    maxPrice = Math.Max(maxPrice, priceDecimal);

                    totalAmount += priceDecimal * quantity;
                    totalQuantity += quantity;

                    for (int i = 0; i < quantity; i++)
                    {
                        count++;

                        var delta = price - mean;
                        mean += delta / count;
                        var delta2 = price - mean;
                        m2 += delta * delta2;
                    }
                }
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
                MaximumPrice = maxPrice,
                MinimumPrice = minPrice,
                Volatility = volatility,
            },
            Timer = new MetricTimer(start, end)
        };

        metric.Data = data;
        metric.SetCalculated();
    }

    private static Expression<Func<SalesFact, bool>> GetWhere(ArticleSalesMetric metric)
    {
        return x => x.SaleContents.Any(z => z.ArticleId == metric.ArticleId)
                    && metric.RangeStart <= x.CreatedAt && x.CreatedAt <= metric.RangeEnd;
    }
}