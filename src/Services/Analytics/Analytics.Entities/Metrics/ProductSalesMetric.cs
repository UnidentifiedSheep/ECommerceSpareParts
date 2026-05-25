using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticleSalesMetric")]
public sealed class ProductSalesMetric : ProductMetric<ProductInfoModel>
{
    private ProductSalesMetric()
    {
    }

    public ProductSalesMetric(int productId) : base(productId)
    {
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale | DependsOn.Period;
}