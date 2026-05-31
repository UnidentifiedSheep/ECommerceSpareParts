using Analytics.Entities.Metrics;
using Analytics.Integration.Tests.DataBuilders.Base;
using Bogus;

namespace Analytics.Integration.Tests.DataBuilders;

public class ProductSalesMetricDataBuilder(Faker faker) 
    : ProductMetricDataBuilderBase<ProductSalesMetricDataBuilder, ProductSalesMetric>(faker)
{
    public override ProductSalesMetric Build()
    {
        var metric = new ProductSalesMetric(ProductId ?? Faker.Random.Int(1));
        FillBase(metric);
        return metric;
    }
}