using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ArticleSaleMetricConverter : MetricConverterBase<ProductSalesMetric>
{
    public override ProductSalesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ArticleId);

        var metric = new ProductSalesMetric(payload.ArticleId.Value);
        FillBase(metric, payload);
        return metric;
    }
}