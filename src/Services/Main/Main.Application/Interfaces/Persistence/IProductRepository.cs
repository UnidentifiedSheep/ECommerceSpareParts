using Application.Common.Interfaces.Repositories;
using Main.Entities.Product;

namespace Main.Application.Interfaces.Persistence;

public interface IProductRepository : IRepository<Product, int>
{
    Task<IReadOnlyList<Product>> GetProductCrosses(
        int productId,
        Criteria<Product> criteria,
        CancellationToken cancellationToken = default);
}