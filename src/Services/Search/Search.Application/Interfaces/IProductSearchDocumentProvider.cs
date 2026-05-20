using Search.Entities;

namespace Search.Application.Interfaces;

public interface IProductSearchDocumentProvider
{
    Task<Product?> GetById(int productId, CancellationToken cancellationToken = default);
}
