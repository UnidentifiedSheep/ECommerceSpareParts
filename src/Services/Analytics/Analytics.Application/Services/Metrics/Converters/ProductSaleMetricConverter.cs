using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ProductSaleMetricConverter : MetricConverterBase<ProductSalesMetric>
{
    public override ProductSalesMetric Convert(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ProductId);

        var metric = new ProductSalesMetric(payload.ProductId.Value);
        FillBase(metric, payload);
        return metric;
    }
}