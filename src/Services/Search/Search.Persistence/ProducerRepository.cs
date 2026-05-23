using Abstractions.Models;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces;
using Search.Application.Interfaces.Producer;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence;

public class ProducerRepository(
    IOptionsMonitor<OpenSearchOptions> options,
    IOpenSearchClient client,
    IIndexInitializer<Producer> idxInitializer) : IProducerRepository
{
    private static readonly Pagination DefaultPagination = new(0, 20);

    public async Task Upsert(
        Producer producer,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.IndexAsync(
            producer,
            i => i
                .Index(idx)
                .Id(producer.Id),
            token);
    }

    public async Task UpsertMany(
        IEnumerable<Producer> producers,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.BulkAsync(
            b => b
                .Index(idx)
                .IndexMany(
                    producers,
                    (d, producer) => d.Id(producer.Id)),
            token);
    }

    public async Task<Producer?> GetById(
        int id,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        var response = await client.GetAsync<Producer>(
            id,
            g => g.Index(idx),
            token);

        return response.Found ? response.Source : null;
    }

    public async Task<IReadOnlyCollection<Producer>> Search(
        string query,
        Pagination? pagination = null,
        CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var page = pagination ?? DefaultPagination;
        var idx = await CheckInitAndGetIdx(token);
        var searchQuery = query.Trim();

        var response = await client.SearchAsync<Producer>(
            s => s
                .Index(idx)
                .From(GetFrom(page))
                .Size(page.Size)
                .Query(q => q.Bool(b => b
                    .Should(
                        sh => sh.Match(m => m
                            .Field(p => p.Name)
                            .Query(searchQuery)
                            .Boost(2)),
                        sh => sh.Match(m => m
                            .Field("otherNames.otherName")
                            .Query(searchQuery)))
                    .MinimumShouldMatch(1))),
            token);

        return response.Documents;
    }

    public async Task Delete(
        int id,
        CancellationToken token = default)
    {
        var idx = await CheckInitAndGetIdx(token);
        await client.DeleteAsync<Producer>(
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
                    b.Delete<Producer>(d => d.Id(id));
                }

                return b;
            },
            token);
    }

    private async Task<string> CheckInitAndGetIdx(CancellationToken token)
    {
        await idxInitializer.LazyInitialize(token);
        return options.CurrentValue.IndexOptions.Producers;
    }

    private static int GetFrom(Pagination pagination)
    {
        return pagination.Page * pagination.Size;
    }
}
