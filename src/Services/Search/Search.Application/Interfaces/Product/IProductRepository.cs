using Abstractions.Models;
using Search.Entities;

namespace Search.Application.Interfaces;

public interface IProductRepository
{
    Task Upsert(Product product, CancellationToken token = default);
    Task UpsertMany(IEnumerable<Product> products, CancellationToken token = default);
    Task<Product?> GetById(int id, CancellationToken token = default);
    Task<IReadOnlyCollection<Product>> Search(
        string query, 
        int? producerId = null, 
        Pagination? pagination = null,
        RangeModel<decimal>? lengthM = null,
        RangeModel<decimal>? widthM = null,
        RangeModel<decimal>? heightM = null,
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> SearchBySku(
        string sku,
        int? producerId,
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByWeightKgRange(
        RangeModel<decimal>? weightKg = null, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByVolumeM3Range(
        RangeModel<decimal>? volumeM3 = null, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task Delete(int id, CancellationToken token = default);
    Task DeleteMany(IEnumerable<int> ids, CancellationToken token = default);
}
