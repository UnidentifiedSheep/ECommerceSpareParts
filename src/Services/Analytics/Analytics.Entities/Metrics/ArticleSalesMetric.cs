using Analytics.Entities.Metrics.JsonDataModels;
using Analytics.Enums;

namespace Analytics.Entities.Metrics;

public sealed class ArticleSalesMetric : Metric<ArticleInfoModel>
{
    public override DependsOn DependsOn { get; protected set; } = DependsOn.Sale | DependsOn.Period;
    public int ArticleId { get; private set; }
    
    public ArticleSalesMetric(Guid createdBy, int articleId) : base(articleId.ToString())
    {
        CreatedBy = createdBy;
        ArticleId = articleId;
    }

}