using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ProductPurchasesMetric")]
public class ProductPurchasesMetric : ProductMetric<ProductInfoModel>
{
    private ProductPurchasesMetric()
    {
    }

    public ProductPurchasesMetric(int productId) : base(productId)
    {
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Purchase;
}