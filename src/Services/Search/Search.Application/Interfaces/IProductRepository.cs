using Search.Entities;

namespace Search.Application.Interfaces;

public interface IProductRepository
{
    Task Upsert(Product product, CancellationToken token = default);
    Task UpsertMany(IEnumerable<Product> products, CancellationToken token = default);
}