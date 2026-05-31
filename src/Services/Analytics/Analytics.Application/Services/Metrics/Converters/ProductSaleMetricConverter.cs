using Analytics.Application.Dtos.CalculationJob;
using Analytics.Entities.Metrics;

namespace Analytics.Application.Services.Metrics.Converters;

public class ProductSaleMetricConverter : MetricConverterBase<ProductSalesMetric>
{
    public override ProductSalesMetric FromPayload(MetricPayloadDto payload)
    {
        ArgumentNullException.ThrowIfNull(payload.ProductId);

        var metric = new ProductSalesMetric(payload.ProductId.Value);
        FillBase(metric, payload);
        return metric;
    }

    public override MetricPayloadDto ToPayload(ProductSalesMetric metric)
    {
        return new MetricPayloadDto
        {
            ProductId = metric.ProductId,
            CurrencyId = metric.CurrencyId,
            RangeEnd = metric.RangeEnd,
            RangeStart = metric.RangeStart
        };
    }
}