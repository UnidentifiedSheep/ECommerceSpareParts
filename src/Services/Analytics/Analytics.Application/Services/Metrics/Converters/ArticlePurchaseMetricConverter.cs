using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ArticlePurchaseMetricConverter : MetricConverterBase<ProductPurchasesMetric>
{
    public override ProductPurchasesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ArticleId);

        var metric = new ProductPurchasesMetric(payload.ArticleId.Value);
        FillBase(metric, payload);
        return metric;
    }
}