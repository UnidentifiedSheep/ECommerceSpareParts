using Analytics.Application.NamedObjects.Metrics.MetricInputBases;
using Analytics.Entities.Metrics;

namespace Analytics.Application.NamedObjects.Metrics;

public record ProductPurchasesMetricInput : ProductMetricInputBase;

public class ProductPurchasesMetricDefinition
    : MetricDefinitionNamedObjectBase<ProductPurchasesMetric, ProductPurchasesMetricInput>
{
    public override string NameLocalizationKey => "product.purchases.metric.name";
    public override string DescriptionLocalizationKey => "product.purchases.metric.description";
    public override string SystemName => nameof(ProductPurchasesMetric);
    public override Type InputType => typeof(ProductPurchasesMetricInput);

    protected override ProductPurchasesMetric CreateMetric(ProductPurchasesMetricInput input)
    {
        return FillMetricBase(new ProductPurchasesMetric(input.ProductId), input);
    }
}