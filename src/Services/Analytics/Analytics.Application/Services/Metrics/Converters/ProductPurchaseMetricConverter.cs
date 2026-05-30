using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ProductPurchaseMetricConverter : MetricConverterBase<ProductPurchasesMetric>
{
    public override ProductPurchasesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ProductId);

        var metric = new ProductPurchasesMetric(payload.ProductId.Value);
        FillBase(metric, payload);
        return metric;
    }
}