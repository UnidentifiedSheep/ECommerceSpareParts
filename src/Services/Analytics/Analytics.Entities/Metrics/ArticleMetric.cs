namespace Analytics.Entities.Metrics;

public abstract class ArticleMetric<TData> : Metric<TData> where TData : class
{
    protected ArticleMetric()
    {
    }

    protected ArticleMetric(int articleId)
    {
        SetArticleId(articleId);
    }

    public int ArticleId { get; private set; }

    private void SetArticleId(int articleId)
    {
        if (articleId <= 0)
            throw new ArgumentException("Article id must be greater than zero.", nameof(articleId));

        ArticleId = articleId;
        SetDimensionKey(articleId.ToString());
    }
}
