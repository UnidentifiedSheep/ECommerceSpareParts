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
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> SearchBySku(
        string sku, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByProducerId(
        int producerId, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByLengthRange(
        RangeModel<decimal>? length = null, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByWidthRange(
        RangeModel<decimal>? width = null, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    
    Task<IReadOnlyCollection<Product>> GetByHeightRange(
        RangeModel<decimal>? height = null, 
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
    
    Task<IReadOnlyCollection<Product>> GetByDimensionsRange(
        RangeModel<decimal>? length = null, 
        RangeModel<decimal>? width = null, 
        RangeModel<decimal>? height = null, 
        Pagination? pagination = null, 
        CancellationToken token = default);
    Task<bool> Exists(int id, CancellationToken token = default);
    Task Delete(int id, CancellationToken token = default);
    Task DeleteMany(IEnumerable<int> ids, CancellationToken token = default);
}