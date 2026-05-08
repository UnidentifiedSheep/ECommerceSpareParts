using Application.Common.Extensions;
using Application.Common.Interfaces.Repositories;
using Domain;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Exceptions.Products;
using Main.Entities.Product;

namespace Main.Application.Extensions;

public static class RepositoryExtensions
{
    public static async Task<Dictionary<TKey, TEntity>> EnsureExistsForUpdateAsync<TEntity, TKey>(
        this IRepository<TEntity, TKey> repository,
        IEnumerable<TKey> ids,
        Func<IReadOnlyList<TKey>, Exception> errorFactory,
        CancellationToken ct = default)
        where TEntity : Entity<TEntity, TKey>
        where TKey : notnull
    {
        var keySet = ids.ToHashSet();

        var criteria = Criteria<TEntity>.New()
            .Track()
            .ForUpdate()
            .Build();

        var result = await repository.FindByIdsAsync(
            keySet,
            criteria,
            ct);

        keySet.EnsureAllExists(
            result.Keys,
            errorFactory);

        return result;
    }

    public static async Task<Product> EnsureProductExistsForUpdateAsync(
        this IProductRepository productRepository,
        int productId,
        CancellationToken cancellationToken = default)
    {
        var criteria = Criteria<Product>.New()
            .Where(x => x.Id == productId)
            .Track()
            .ForUpdate()
            .Build();

        return await productRepository.FirstOrDefaultAsync(criteria, cancellationToken)
               ?? throw new ProductNotFoundException(productId);
    }
}