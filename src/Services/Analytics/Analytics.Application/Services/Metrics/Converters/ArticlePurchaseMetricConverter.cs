using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ArticlePurchaseMetricConverter : MetricConverterBase<ArticlePurchasesMetric>
{
    public override ArticlePurchasesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ArticleId);

        var metric = new ArticlePurchasesMetric(payload.ArticleId.Value);
        FillBase(metric, payload);
        return metric;
    }
}