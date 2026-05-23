using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;

namespace Search.Application.Services;

public class ProductIndexSynchronizer(
    IProductSearchDocumentProvider productSearchDocumentProvider,
    IProductRepository productRepository) : IProductIndexSynchronizer
{
    public async Task Reindex(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await productSearchDocumentProvider.GetById(productId, cancellationToken);

        if (product == null)
        {
            await productRepository.Delete(productId, cancellationToken);
            return;
        }

        await productRepository.Upsert(product, cancellationToken);
    }

    public Task Delete(
        int productId,
        CancellationToken cancellationToken = default)
    {
        return productRepository.Delete(productId, cancellationToken);
    }
}
