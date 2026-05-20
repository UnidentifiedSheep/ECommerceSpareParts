using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence;

public class ProductRepository(
    IOptions<OpenSearchOptions> options,
    IOpenSearchClient client,
    IIndexInitializer<Product> idxInitializer) : IProductRepository
{
    public async Task Upsert(
        Product product,
        CancellationToken token = default)
    {
        await idxInitializer.LazyInitialize(token);
        string idx = options.Value.IndexOptions.Products;
        await client.IndexAsync(
            product, 
            i => i
                .Index(idx)
                .Id(product.Id), 
            token);
    }

    public async Task UpsertMany(
        IEnumerable<Product> products,
        CancellationToken token = default)
    {
        await idxInitializer.LazyInitialize(token);
        string idx = options.Value.IndexOptions.Products;
        await client.BulkAsync(
            b => b
                .Index(idx)
                .IndexMany(
                    products, 
                    (d, product) => d.Id(product.Id)), 
            token);
    }
    
    
}