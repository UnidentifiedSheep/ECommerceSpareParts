using Analytics.Application.NamedObjects.Metrics.MetricInputBases;
using Analytics.Entities.Metrics;

namespace Analytics.Application.NamedObjects.Metrics;

public record ProductSalesMetricInput : ProductMetricInputBase;

public class ProductSalesMetricDefinition
    : MetricDefinitionNamedObjectBase<ProductSalesMetric, ProductSalesMetricInput>
{
    public override string NameLocalizationKey => "product.sales.metric.name";
    public override string DescriptionLocalizationKey => "product.sales.metric.description";
    public override string SystemName => nameof(ProductSalesMetric);
    public override Type InputType => typeof(ProductSalesMetricInput);

    protected override ProductSalesMetric CreateMetric(ProductSalesMetricInput input)
    {
        return FillMetricBase(new ProductSalesMetric(input.ProductId), input);
    }
}