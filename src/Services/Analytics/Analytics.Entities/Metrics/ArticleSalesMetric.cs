using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticleSalesMetric")]
public sealed class ArticleSalesMetric : ArticleMetric<ArticleInfoModel>
{
    public ArticleSalesMetric(int articleId) : base(articleId)
    {
        ArticleId = articleId;
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale | DependsOn.Period;
}