using Abstractions.Models;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces;
using Search.Entities;
using Search.Persistence.Interfaces;
using System.Linq.Expressions;

namespace Search.Persistence;

public class ProductRepository(
    IOptionsMonitor<OpenSearchOptions> options,
    IOpenSearchClient client,
    IIndexInitializer<Product> idxInitializer) : IProductRepository
{
    private static readonly Pagination DefaultPagination = new(0, 20);

    public async Task Upsert(
        Product product,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.IndexAsync(
            product,
            i => i
                .Index(idx)
                .Id(product.Id),
            token);
    }

    public async Task UpsertMany(
        IEnumerable<Product> products,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.BulkAsync(
            b => b
                .Index(idx)
                .IndexMany(
                    products,
                    (d, product) => d.Id(product.Id)),
            token);
    }

    public async Task<Product?> GetById(
        int id,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.GetAsync<Product>(
            id,
            g => g.Index(idx),
            token);

        return response.Found ? response.Source : null;
    }

    public async Task<IReadOnlyCollection<Product>> Search(
        string query,
        int? producerId = null,
        Pagination? pagination = null,
        RangeModel<decimal>? lengthM = null,
        RangeModel<decimal>? widthM = null,
        RangeModel<decimal>? heightM = null,
        CancellationToken token = default)
    {
        var hasTextQuery = !string.IsNullOrWhiteSpace(query);
        var filters = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();

        if (producerId.HasValue)
        {
            filters.Add(f => f.Term(t => t
                .Field(p => p.ProducerId)
                .Value(producerId.Value)));
        }

        AddRangeFilter(filters, p => p.Dimensions!.LengthM, lengthM);
        AddRangeFilter(filters, p => p.Dimensions!.WidthM, widthM);
        AddRangeFilter(filters, p => p.Dimensions!.HeightM, heightM);

        if (!hasTextQuery && filters.Count == 0)
            return [];

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var normalizedQuery = ProductSkuNormalizer.Normalize(query);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b =>
                {
                    if (hasTextQuery)
                    {
                        var should = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
                        AddSkuQueries(should, normalizedQuery);
                        should.Add(sh => sh.Match(m => m
                            .Field(p => p.Name)
                            .Query(query)
                            .Boost(2)));
                        should.Add(sh => sh.MatchPhrasePrefix(m => m
                            .Field(p => p.Name)
                            .Query(query)));

                        b = b.Should(should.ToArray())
                            .MinimumShouldMatch(1);
                    }

                    return filters.Count > 0
                        ? b.Filter(filters)
                        : b;
                })),
            token);

        return response.Documents;
    }

    public async Task<IReadOnlyCollection<Product>> SearchBySku(
        string sku,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return [];
        

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var normalizedSku = ProductSkuNormalizer.Normalize(sku);
        if (string.IsNullOrEmpty(normalizedSku))
            return [];
        

        var should = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
        AddSkuQueries(should, normalizedSku);

        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b => b
                    .Should(should.ToArray())
                    .MinimumShouldMatch(1))),
            token);

        return response.Documents;
    }

    public async Task<IReadOnlyCollection<Product>> GetByProducerId(
        int producerId,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Term(t => t
                    .Field(p => p.ProducerId)
                    .Value(producerId))),
            token);

        return response.Documents;
    }

    public Task<IReadOnlyCollection<Product>> GetByLengthRange(
        RangeModel<decimal>? length = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.LengthM, length, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByWidthRange(
        RangeModel<decimal>? width = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.WidthM, width, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByHeightRange(
        RangeModel<decimal>? height = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.HeightM, height, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByWeightKgRange(
        RangeModel<decimal>? weightKg = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Weight!.WeightKg, weightKg, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByVolumeM3Range(
        RangeModel<decimal>? volumeM3 = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.VolumeM3, volumeM3, pagination, token);
    }

    public async Task<IReadOnlyCollection<Product>> GetByDimensionsRange(
        RangeModel<decimal>? length = null,
        RangeModel<decimal>? width = null,
        RangeModel<decimal>? height = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        var filters = new List<Func<QueryContainerDescriptor<Product>, QueryContainer>>();
        AddRangeFilter(filters, p => p.Dimensions!.LengthM, length);
        AddRangeFilter(filters, p => p.Dimensions!.WidthM, width);
        AddRangeFilter(filters, p => p.Dimensions!.HeightM, height);

        if (filters.Count == 0)
            return [];

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b => b.Filter(filters))),
            token);

        return response.Documents;
    }

    public async Task<bool> Exists(
        int id,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.DocumentExistsAsync<Product>(
            id,
            e => e.Index(idx),
            token);

        return response.Exists;
    }

    public async Task Delete(
        int id,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.DeleteAsync<Product>(
            id,
            d => d.Index(idx),
            token);
    }

    public async Task DeleteMany(
        IEnumerable<int> ids,
        CancellationToken token = default)
    {
        var idList = ids.ToArray();
        if (idList.Length == 0)
            return;
        

        var idx = await CheckInitAndGetIdx(token);
        await client.BulkAsync(
            b =>
            {
                b.Index(idx);

                foreach (var id in idList)
                {
                    b.Delete<Product>(d => d.Id(id));
                }

                return b;
            },
            token);
    }

    private async Task<string> CheckInitAndGetIdx(CancellationToken token)
    {
        await idxInitializer.LazyInitialize(token);
        return options.CurrentValue.IndexOptions.Products;
    }

    private async Task<IReadOnlyCollection<Product>> SearchByRange(
        Expression<Func<Product, object>> field,
        RangeModel<decimal>? range,
        Pagination? pagination,
        CancellationToken token)
    {
        if (range is not { HasBounds: true })
            return [];

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Range(r => ApplyRange(r.Field(field), range))),
            token);

        return response.Documents;
    }

    private static void AddRangeFilter(
        ICollection<Func<QueryContainerDescriptor<Product>, QueryContainer>> filters,
        Expression<Func<Product, object>> field,
        RangeModel<decimal>? range)
    {
        if (range is not { HasBounds: true })
            return;

        filters.Add(q => q.Range(r => ApplyRange(r.Field(field), range)));
    }

    private static NumericRangeQueryDescriptor<Product> ApplyRange(
        NumericRangeQueryDescriptor<Product> descriptor,
        RangeModel<decimal> range)
    {
        if (range.Min.HasValue)
        {
            descriptor = descriptor.GreaterThanOrEquals((double)range.Min.Value);
        }

        if (range.Max.HasValue)
        {
            descriptor = descriptor.LessThanOrEquals((double)range.Max.Value);
        }

        return descriptor;
    }

    private static int GetFrom(Pagination pagination)
    {
        return pagination.Page * pagination.Size;
    }

    private static void AddSkuQueries(
        ICollection<Func<QueryContainerDescriptor<Product>, QueryContainer>> queries,
        string normalizedSku)
    {
        if (string.IsNullOrEmpty(normalizedSku))
        {
            return;
        }

        queries.Add(q => q.Term(t => t
            .Field(p => p.NormalizedSku)
            .Value(normalizedSku)
            .Boost(10)));

        queries.Add(q => q.Prefix(p => p
            .Field(k => k.NormalizedSku)
            .Value(normalizedSku)
            .Boost(6)));

        queries.Add(q => q.Wildcard(w => w
            .Field(p => p.NormalizedSku)
            .Value($"*{normalizedSku}*")
            .Boost(3)));

        queries.Add(q => q.Fuzzy(f => f
            .Field(p => p.NormalizedSku)
            .Value(normalizedSku)
            .Fuzziness(Fuzziness.Auto)
            .Boost(1)));
    }
}
