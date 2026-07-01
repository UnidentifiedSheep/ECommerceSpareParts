namespace Search.Application.Interfaces.Product;

public interface IProductSearchDocumentProvider
{
    Task<Entities.Product?> GetById(int productId, CancellationToken cancellationToken = default);
}