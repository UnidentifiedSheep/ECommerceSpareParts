using Application.Common.Interfaces.Repositories;
using Main.Abstractions.Exceptions.Articles;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task<IReadOnlyList<Product>> EnsureProductsExistsForUpdateAsync(
        this IProductRepository productRepository,
        IEnumerable<int> articleIds,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = articleIds.ToHashSet();
        var criteria = Criteria<Product>.New()
            .ForUpdate()
            .Track()
            .Where(x => requestedIds.Contains(x.Id))
            .Build();
        
        var found = await productRepository.ListAsync(criteria, cancellationToken);

        foreach (var id in found.Select(x => x.Id)) requestedIds.Remove(id);
        return requestedIds.Count != 0 
            ? throw new ProductNotFoundException(requestedIds) 
            : found;
    }
}