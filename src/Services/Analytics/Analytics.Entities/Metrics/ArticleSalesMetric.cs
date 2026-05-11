using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticleSalesMetric")]
public sealed class ArticleSalesMetric : ArticleMetric<ArticleInfoModel>
{
    private ArticleSalesMetric()
    {
    }

    public ArticleSalesMetric(int articleId) : base(articleId)
    {
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale | DependsOn.Period;
}