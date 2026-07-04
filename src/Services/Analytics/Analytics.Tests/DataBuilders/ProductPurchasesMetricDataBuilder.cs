using Analytics.Entities.Metrics;
using Analytics.Integration.Tests.DataBuilders.Base;
using Bogus;

namespace Analytics.Integration.Tests.DataBuilders;

public class ProductPurchasesMetricDataBuilder(Faker faker)
    : ProductMetricDataBuilderBase<ProductPurchasesMetricDataBuilder, ProductPurchasesMetric>(faker)
{
    public override ProductPurchasesMetric Build()
    {
        var metric = new ProductPurchasesMetric(ProductId ?? Faker.Random.Int(1));
        FillBase(metric);
        return metric;
    }
}