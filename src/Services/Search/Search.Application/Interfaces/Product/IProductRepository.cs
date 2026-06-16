using Abstractions.Models;

namespace Search.Application.Interfaces.Product;

public interface IProductRepository
{
    Task Upsert(Entities.Product product, CancellationToken token = default);
    Task UpsertMany(IEnumerable<Entities.Product> products, CancellationToken token = default);
    Task<Entities.Product?> GetById(int id, CancellationToken token = default);
    Task<IReadOnlyCollection<Entities.Product>> Search(
        string query, 
        int? producerId = null, 
        Pagination? pagination = null,
        string? sortBy = null,
        RangeModel<decimal>? lengthM = null,
        RangeModel<decimal>? widthM = null,
        RangeModel<decimal>? heightM = null,
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Entities.Product>> SearchBySku(
        string sku,
        int? producerId,
        Pagination? pagination = null,
        string? sortBy = null,
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Entities.Product>> GetByWeightKgRange(
        RangeModel<decimal>? weightKg = null, 
        Pagination? pagination = null,
        string? sortBy = null,
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Entities.Product>> GetByVolumeM3Range(
        RangeModel<decimal>? volumeM3 = null, 
        Pagination? pagination = null,
        string? sortBy = null,
        CancellationToken token = default);
    
    Task Delete(int id, CancellationToken token = default);
    Task DeleteMany(IEnumerable<int> ids, CancellationToken token = default);
}
