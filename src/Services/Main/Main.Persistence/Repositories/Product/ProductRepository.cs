using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Repositories;
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
        return await Context.Products
            .FromSql($"""
                      SELECT Distinct on (a.id) a.* 
                      FROM products a 
                      JOIN product_crosses c ON a.id = c.left_product OR a.id = c.right_product 
                                             WHERE c.left_product = {productId} OR 
                                                 c.right_product = {productId}
                      ORDER BY a.id
                      """)
            .Apply(criteria)
            .ToListAsync(cancellationToken);
    }
}