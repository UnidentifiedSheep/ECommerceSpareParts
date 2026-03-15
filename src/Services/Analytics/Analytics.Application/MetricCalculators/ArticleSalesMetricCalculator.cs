using System.Linq.Expressions;
using Abstractions.Interfaces.Currency;
using Analytics.Abstractions.Interfaces.DbRepositories;
using Analytics.Entities;
using Analytics.Entities.Metrics;
using Analytics.Entities.Metrics.JsonDataModels;

namespace Analytics.Application.MetricCalculators;

public class ArticleSalesMetricCalculator(ISalesRepository salesRepository, ICurrencyConverter currencyConverter) 
    : MetricCalculatorBase<ArticleSalesMetric>
{
    public override async Task CalculateMetric(ArticleSalesMetric metric, CancellationToken cancellationToken = default)
    {
        decimal minPrice = decimal.MaxValue;
        decimal maxPrice = decimal.MinValue;
        decimal totalAmount = 0;
        int totalQuantity = 0;

        var (start, end) = await WithTimer(async () =>
        {
            await foreach (var fact in salesRepository.GetFacts(GetWhere(metric)).WithCancellation(cancellationToken))
            {
                var neededArticle = fact.SaleContents
                    .Where(x => x.ArticleId == metric.ArticleId)
                    .Select(x => (currencyConverter.ConvertToUsd(x.Price, fact.CurrencyId), x.Count))
                    .ToList();

                minPrice = Math.Min(minPrice, neededArticle.Min(x => x.Item1));
                maxPrice = Math.Max(maxPrice, neededArticle.Max(x => x.Item1));
                totalAmount += neededArticle.Sum(x => x.Count * x.Item1);
                totalQuantity += neededArticle.Sum(x => x.Count);
            }
        });
        
        decimal avgPrice = totalAmount / totalQuantity;

        var data = new ArticleInfoModel
        {
            Quantity = totalQuantity,
            TotalAmount = totalAmount,
            PriceInfo = new PriceInfoModel
            {
                AveragePrice = avgPrice,
                MaximumPrice = maxPrice,
                MinimumPrice = minPrice,
            },
            Timer = new MetricTimer(start, end),
        };
        
        metric.Data = data;
        metric.SetCalculated();
    }
    
    
    private static Expression<Func<SalesFact, bool>> GetWhere(ArticleSalesMetric metric)
        => x => x.SaleContents.Any(z => z.ArticleId == metric.ArticleId) 
                && metric.RangeStart <= x.CreatedAt && x.CreatedAt <= metric.RangeEnd;
}