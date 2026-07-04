using Search.Application.Interfaces;
using Search.Application.Interfaces.Product;
using Search.Entities;

namespace Search.Application.Services.IndexSynchronizers;

public class ProductIndexSynchronizer(
    IProductSearchDocumentProvider productSearchDocumentProvider,
    IProductRepository productRepository
) : IIndexSynchronizer<Product, int>
{
    public async Task Reindex(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var products = await productSearchDocumentProvider
            .GetByIds(ids, cancellationToken);

        var toDelete = new List<int>();
        var toUpsert = new List<Product>();

        foreach (var (id, product) in products)
        {
            if (product == null)
                toDelete.Add(id);
            else
                toUpsert.Add(product);
        }

        await productRepository.DeleteMany(toDelete, cancellationToken);
        await productRepository.UpsertMany(toUpsert, cancellationToken);
    }

    public Task Delete(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        return productRepository.DeleteMany(ids, cancellationToken);
    }
}