using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;
using Search.Entities;

namespace Search.Application.Services.IndexSynchronizers;

public class ProductIndexSynchronizer(
    IProductSearchDocumentProvider productSearchDocumentProvider,
    IProductRepository productRepository
) : IIndexSynchronizer<Product, int>
{
    public async Task Reindex(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await productSearchDocumentProvider.GetById(id, cancellationToken);

        if (product == null)
        {
            await productRepository.Delete(id, cancellationToken);
            return;
        }

        await productRepository.Upsert(product, cancellationToken);
    }

    public Task Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        return productRepository.Delete(id, cancellationToken);
    }
}