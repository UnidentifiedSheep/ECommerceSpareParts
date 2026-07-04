using Analytics.Entities.Metrics;
using Bogus;

namespace Analytics.Integration.Tests.DataBuilders.Base;

public abstract class ProductMetricDataBuilderBase<T, TMetric>(Faker faker)
    : MetricDataBuilderBase<T, TMetric>(faker)
    where T : ProductMetricDataBuilderBase<T, TMetric>
    where TMetric : Metric
{
    public int? ProductId { get; private set; }

    public T WithProductId(int productId)
    {
        ProductId = productId;
        return (T)this;
    }
}