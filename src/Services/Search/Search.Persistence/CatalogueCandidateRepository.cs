using Microsoft.Extensions.Options;
using OpenSearch.Client;
using Search.Abstractions.Options;
using Search.Application.Interfaces.CatalogueCandidate;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence;

public sealed class CatalogueCandidateRepository(
    IOptionsMonitor<OpenSearchOptions> options,
    IOpenSearchClient client,
    IIndexInitializer<CatalogueCandidate> indexInitializer)
    : ICatalogueCandidateRepository
{
    public async Task UpsertMany(
        IEnumerable<CatalogueCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        var documents = candidates
            .DistinctBy(x => x.Id)
            .ToArray();
        if (documents.Length == 0) return;

        var index = await CheckInitAndGetIndex(cancellationToken);
        var response = await client.BulkAsync(
            bulk => bulk
                .Index(index)
                .IndexMany(
                    documents,
                    (descriptor, candidate) => descriptor.Id(candidate.Id)),
            cancellationToken);

        EnsureBulkSucceeded(response, "upsert");
    }

    public async Task DeleteMany(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = ids.Distinct().ToArray();
        if (distinctIds.Length == 0) return;

        var index = await CheckInitAndGetIndex(cancellationToken);
        var response = await client.BulkAsync(
            bulk =>
            {
                bulk.Index(index);

                foreach (var id in distinctIds)
                    bulk.Delete<CatalogueCandidate>(descriptor => descriptor.Id(id));

                return bulk;
            },
            cancellationToken);

        EnsureBulkSucceeded(response, "delete");
    }

    private async Task<string> CheckInitAndGetIndex(
        CancellationToken cancellationToken)
    {
        await indexInitializer.LazyInitialize(cancellationToken);
        return options.CurrentValue.IndexOptions.CatalogueCandidates;
    }

    private static void EnsureBulkSucceeded(
        BulkResponse response,
        string operation)
    {
        if (response is { IsValid: true, Errors: false }) return;

        var itemErrors = string.Join(
            "; ",
            response.ItemsWithErrors.Select(item =>
                $"{item.Id}: {item.Error?.Reason ?? item.Status.ToString()}"));
        var details = string.IsNullOrWhiteSpace(itemErrors)
            ? response.ServerError?.Error?.Reason ?? response.DebugInformation
            : itemErrors;

        throw new InvalidOperationException(
            $"Failed to {operation} catalogue candidate documents. {details}");
    }
}
