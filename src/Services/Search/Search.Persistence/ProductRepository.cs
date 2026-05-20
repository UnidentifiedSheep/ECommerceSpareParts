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
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<Product>();
        }

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var normalizedQuery = NormalizeSku(query);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b =>
                {
                    b = b.Should(
                            sh => sh.Term(t => t
                                .Field(p => p.Sku)
                                .Value(normalizedQuery)
                                .Boost(8)),
                            sh => sh.Prefix(p => p
                                .Field(k => k.Sku)
                                .Value(normalizedQuery)
                                .Boost(4)),
                            sh => sh.Match(m => m
                                .Field(p => p.Name)
                                .Query(query)
                                .Boost(2)),
                            sh => sh.MatchPhrasePrefix(m => m
                                .Field(p => p.Name)
                                .Query(query)))
                        .MinimumShouldMatch(1);

                    return producerId.HasValue
                        ? b.Filter(f => f.Term(t => t
                            .Field(p => p.ProducerId)
                            .Value(producerId.Value)))
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
        {
            return Array.Empty<Product>();
        }

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var normalizedSku = NormalizeSku(sku);
        var response = await client.SearchAsync<Product>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b => b.Should(
                        sh => sh.Term(t => t
                            .Field(p => p.Sku)
                            .Value(normalizedSku)
                            .Boost(8)),
                        sh => sh.Prefix(p => p
                            .Field(k => k.Sku)
                            .Value(normalizedSku)
                            .Boost(4)))
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
        return SearchByRange(p => p.Dimensions!.Length, length, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByWidthRange(
        RangeModel<decimal>? width = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.Width, width, pagination, token);
    }

    public Task<IReadOnlyCollection<Product>> GetByHeightRange(
        RangeModel<decimal>? height = null,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        return SearchByRange(p => p.Dimensions!.Height, height, pagination, token);
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
        AddRangeFilter(filters, p => p.Dimensions!.Length, length);
        AddRangeFilter(filters, p => p.Dimensions!.Width, width);
        AddRangeFilter(filters, p => p.Dimensions!.Height, height);

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

    private static string NormalizeSku(string sku)
    {
        return sku.Trim().ToLowerInvariant();
    }
}
