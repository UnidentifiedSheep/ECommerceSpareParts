using Analytics.Attributes;
using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

[MetricInfo("ArticlePurchasesMetric")]
public class ArticlePurchasesMetric : ArticleMetric<ArticleInfoModel>
{
    public ArticlePurchasesMetric(int articleId) : base(articleId)
    {
        ArticleId = articleId;
    }

    public override DependsOn DependsOn { get; protected set; } = DependsOn.Purchase | DependsOn.Period;
    
}