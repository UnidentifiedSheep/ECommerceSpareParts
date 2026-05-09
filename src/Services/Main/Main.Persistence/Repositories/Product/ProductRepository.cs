using Application.Common.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ProductRepository(DContext context) : RepositoryBase<DContext, Product, int>(context), IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetProductCrosses(
        int productId,
        Criteria<Product> criteria,
        CancellationToken cancellationToken = default)
    {
        var left = Context.ProductCrosses
            .Where(c => c.RightProductId == productId)
            .Select(c => c.LeftProduct);

        var right = Context.ProductCrosses
            .Where(c => c.LeftProductId == productId)
            .Select(c => c.RightProduct);

        return await left
            .Union(right)
            .Apply(criteria)
            .ToListAsync(cancellationToken);
    }

    public Task UpsertProductCrosses(
        IEnumerable<ProductCross> crosses, 
        CancellationToken cancellationToken = default)
    {
        return Context.BulkInsertOrUpdateAsync(
            entities: crosses, 
            cancellationToken: cancellationToken);
    }

    public override Task<Dictionary<int, Product>> FindByIdsAsync(
        IEnumerable<int> ids,
        Criteria<Product>? criteria = null,
        CancellationToken ct = default)
    {
        return Context.Products
            .Apply(criteria)
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
    }
}