using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo(nameof(ProductSalesMetric))]
public sealed class ProductSalesMetric : ProductMetric<ProductInfoModel>
{
    public static string SystemName = nameof(ProductSalesMetric);
    private ProductSalesMetric()
    {
    }

    public ProductSalesMetric(int productId) : base(productId)
    {
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale;
}