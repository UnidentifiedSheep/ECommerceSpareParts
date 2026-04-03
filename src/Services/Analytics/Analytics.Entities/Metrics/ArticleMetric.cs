namespace Analytics.Entities.Metrics;

public abstract class ArticleMetric<TData> : Metric<TData> where TData : class
{
    protected ArticleMetric(int articleId)
    {
        ArticleId = articleId;
        DimensionKey = articleId.ToString();
    }

    public int ArticleId { get; protected set; }
}