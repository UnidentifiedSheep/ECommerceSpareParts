namespace Analytics.Entities.Metrics;

public abstract class ProductMetric<TData> : Metric<TData> where TData : class
{
    protected ProductMetric() { }

    protected ProductMetric(int productId) { SetProductId(productId); }

    public int ProductId { get; private set; }

    private void SetProductId(int productId)
    {
        if (productId <= 0)
            throw new ArgumentException("Product id must be greater than zero.", nameof(productId));

        ProductId = productId;
        SetDimensionKey(productId.ToString());
    }
}