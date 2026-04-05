using Analytics.Abstractions.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ArticleSaleMetricConverter : MetricConverterBase<ArticleSalesMetric>
{
    public override ArticleSalesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ArticleId);

        var metric = new ArticleSalesMetric(payload.ArticleId.Value);
        FillBase(metric, payload);
        return metric;
    }
}