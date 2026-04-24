using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Products;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Product;
using Main.Entities.Storage;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task<Dictionary<int, Product>> EnsureProductsExistsForUpdateAsync(
        this IRepository<Product, int> productRepository,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = productIds.ToHashSet();
        var criteria = Criteria<Product>.New()
            .ForUpdate()
            .Track()
            .Where(x => requestedIds.Contains(x.Id))
            .Build();
        
        var found = (await productRepository.ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.Id);

        foreach (var id in found.Keys) requestedIds.Remove(id);
        return requestedIds.Count != 0 
            ? throw new ProductNotFoundException(requestedIds) 
            : found;
    }

    public static async Task<Product> EnsureProductExistsForUpdateAsync(
        this IProductRepository productRepository,
        int productId,
        CancellationToken cancellationToken = default
    )
    {
        var criteria = Criteria<Product>.New()
            .Where(x => x.Id == productId)
            .Track()
            .ForUpdate()
            .Build();
        
        return await productRepository.FirstOrDefaultAsync(criteria, cancellationToken)
            ?? throw new ProductNotFoundException(productId);
    }

    public static async Task<Dictionary<int, StorageContent>> EnsureStorageContentsExistsForUpdateAsync(
        this IRepository<StorageContent, int> repository,
        IEnumerable<int> storageContentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = storageContentIds.ToHashSet();
        var criteria = Criteria<StorageContent>.New()
            .Where(x => ids.Contains(x.Id))
            .ForUpdate()
            .Track()
            .Build();

        var found = (await repository.ListAsync(criteria, cancellationToken))
            .ToDictionary(x => x.Id);
        foreach (var id in found.Keys)
            ids.Remove(id);

        return ids.Count != 0
            ? throw new StorageContentNotFoundException(ids)
            : found;
    }
}