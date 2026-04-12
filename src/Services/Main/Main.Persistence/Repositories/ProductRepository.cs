using Application.Common.Interfaces.Repositories;
using Main.Application.Interfaces.Repositories;
using Main.Entities.Product;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Extensions;

namespace Main.Persistence.Repositories;

public class ProductRepository(DContext context) : IProductRepository
{
    public async ValueTask<Product?> GetById(
        int id, 
        CancellationToken ct = default)
    {
        return await context.Products.FindAsync([id], ct);
    }

    public async Task<Product?> FirstOrDefaultAsync(Criteria<Product>? criteria = null, CancellationToken ct = default)
    {
        var query = context.Products.AsQueryable();
        if (criteria != null)
            query = query.Apply(criteria);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<Product>> ListAsync(Criteria<Product>? criteria = null, CancellationToken ct = default)
    {
        var query = context.Products.AsQueryable();
        if (criteria != null)
            query = query.Apply(criteria);
        return await query.ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<Product>> GetProductCrosses(
        int productId,
        Criteria<Product> criteria,
        CancellationToken cancellationToken = default)
    {
        return await context.Products
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