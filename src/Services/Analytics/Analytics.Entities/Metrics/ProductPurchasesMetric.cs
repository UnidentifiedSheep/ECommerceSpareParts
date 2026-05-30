using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo(nameof(ProductPurchasesMetric))]
public class ProductPurchasesMetric : ProductMetric<ProductInfoModel>
{
    public static string SystemName => nameof(ProductPurchasesMetric);
    private ProductPurchasesMetric()
    {
    }

    public ProductPurchasesMetric(int productId) : base(productId)
    {
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Purchase;
}