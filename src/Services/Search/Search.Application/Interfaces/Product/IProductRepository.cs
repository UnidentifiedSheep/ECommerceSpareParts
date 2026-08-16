using Abstractions.Models;
using Search.Application.Models.CatalogueSearch;
using Search.Enums;

namespace Search.Application.Interfaces.Product;

public interface IProductRepository
    : ISearchRepository<Entities.Product, int>
{
    Task<SearchResult<Entities.Product>> Search(
        CatalogueSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Entities.Product>> Search(
        string query,
        int? producerId = null,
        Pagination? pagination = null,
        string[]? sortBy = null,
        RangeModel<decimal>? lengthM = null,
        RangeModel<decimal>? widthM = null,
        RangeModel<decimal>? heightM = null,
        CancellationToken token = default);

    Task<IReadOnlyCollection<Entities.Product>> SearchBySku(
        string? sku,
        int? producerId,
        SkuSearchMode searchMode,
        Pagination? pagination = null,
        string[]? sortBy = null,
        CancellationToken token = default);

    Task<IReadOnlyCollection<Entities.Product>> GetByWeightKgRange(
        RangeModel<decimal>? weightKg = null,
        Pagination? pagination = null,
        string[]? sortBy = null,
        CancellationToken token = default);

    Task<IReadOnlyCollection<Entities.Product>> GetByVolumeM3Range(
        RangeModel<decimal>? volumeM3 = null,
        Pagination? pagination = null,
        string[]? sortBy = null,
        CancellationToken token = default);

}
