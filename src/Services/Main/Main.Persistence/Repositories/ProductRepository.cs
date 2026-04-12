using Domain.Interfaces;
using Main.Entities.Product;
using Main.Persistence.Context;

namespace Main.Persistence.Repositories;

public class ProductRepository(DContext context) : IRepository<Product, int>
{
    public async ValueTask<Product?> GetById(
        int id, 
        ISpecification<Product, int>? spec = null, 
        CancellationToken ct = default)
    {
        var query = context.Products;
        spec?.Apply(query);
        
        return await query.FindAsync([id], cancellationToken: ct);
    }
}