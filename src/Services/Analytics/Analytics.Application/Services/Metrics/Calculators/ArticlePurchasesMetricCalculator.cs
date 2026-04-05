using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Calculators;

public class ArticlePurchasesMetricCalculator : MetricCalculatorBase<ArticlePurchasesMetric>
{
    public override async Task CalculateMetric(ArticlePurchasesMetric metric, CancellationToken cancellationToken = default)
    {
        
    }
}