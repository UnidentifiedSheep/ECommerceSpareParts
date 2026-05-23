namespace Search.Application.Interfaces.Product;

public interface IProductIndexSynchronizer
{
    Task Reindex(int productId, CancellationToken cancellationToken = default);

    Task Delete(int productId, CancellationToken cancellationToken = default);
}
