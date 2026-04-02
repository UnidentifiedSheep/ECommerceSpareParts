using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticlePurchasesMetric")]
public class ArticlePurchasesMetric : Metric<ArticleInfoModel>
{
    public ArticlePurchasesMetric(Guid createdBy, int articleId) : base(articleId.ToString())
    {
        CreatedBy = createdBy;
        ArticleId = articleId;
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Purchase | DependsOn.Period;
    public int ArticleId { get; private set; }
}