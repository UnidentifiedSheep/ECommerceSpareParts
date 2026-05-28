using Application.Common.Interfaces.Repositories;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;

using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Product;

public class ProductRepository(DContext context, QueryExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.Product.Product, int>(context, extensions), IProductRepository
{
    public async Task<IReadOnlyList<Entities.Product.Product>> GetProductCrosses(
        int productId,
        Criteria<Entities.Product.Product> criteria,
        CancellationToken cancellationToken = default)
    {
        var left = Context.ProductCrosses
            .Where(c => c.RightProductId == productId)
            .Select(c => c.LeftProduct);

        var right = Context.ProductCrosses
            .Where(c => c.LeftProductId == productId)
            .Select(c => c.RightProduct);

        return await QueryableExtensions.Apply(left.Union(right), criteria)
            .ToListAsync(cancellationToken);
    }

    public Task UpsertProductCrosses(
        IEnumerable<ProductCross> crosses,
        CancellationToken cancellationToken = default)
    {
        return Context.BulkInsertOrUpdateAsync(
            crosses,
            cancellationToken: cancellationToken);
    }
}