namespace Analytics.Entities.Metrics;

public abstract class ProductMetric<TData> : Metric<TData> where TData : class
{
    protected ProductMetric()
    {
    }

    protected ProductMetric(int productId)
    {
        SetArticleId(productId);
    }

    public int ProductId { get; private set; }

    private void SetArticleId(int articleId)
    {
        if (articleId <= 0)
            throw new ArgumentException("Article id must be greater than zero.", nameof(articleId));

        ProductId = articleId;
        SetDimensionKey(articleId.ToString());
    }
}