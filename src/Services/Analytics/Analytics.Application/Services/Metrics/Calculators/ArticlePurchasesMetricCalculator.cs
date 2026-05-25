using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Calculators;

public class ArticlePurchasesMetricCalculator : MetricCalculatorBase<ProductPurchasesMetric>
{
    public override async Task CalculateMetric(
        ProductPurchasesMetric metric,
        CancellationToken cancellationToken = default)
    {
    }
}