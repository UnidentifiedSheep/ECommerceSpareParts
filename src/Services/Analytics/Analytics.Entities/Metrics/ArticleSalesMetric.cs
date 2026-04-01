using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticleSalesMetric")]
public sealed class ArticleSalesMetric : Metric<ArticleInfoModel>
{
    public ArticleSalesMetric(Guid createdBy, int articleId) : base(articleId.ToString())
    {
        CreatedBy = createdBy;
        ArticleId = articleId;
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale | DependsOn.Period;
    public int ArticleId { get; private set; }
}