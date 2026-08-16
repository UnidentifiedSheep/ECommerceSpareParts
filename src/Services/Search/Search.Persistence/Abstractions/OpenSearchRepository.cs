using OpenSearch.Client;
using Search.Application.Interfaces;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Abstractions;

public abstract class OpenSearchRepository<TDocument, TKey>(
    IOpenSearchClient client,
    IIndexInitializer<TDocument> indexInitializer,
    Func<string> indexProvider,
    Func<TDocument, TKey> idSelector)
    : ISearchRepository<TDocument, TKey>
    where TDocument : class
    where TKey : notnull
{
    protected IOpenSearchClient Client => client;

    public async Task<TDocument?> GetById(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var index = await GetIndex(cancellationToken);
        var response = await client.GetAsync<TDocument>(
            id.ToString(),
            descriptor => descriptor.Index(index),
            cancellationToken);

        if (response.Found) return response.Source;
        if (response.ApiCall?.HttpStatusCode == 404) return null;

        EnsureResponseSucceeded(
            response,
            $"get '{id}' from");
        return null;
    }

    public async Task Upsert(
        TDocument document,
        CancellationToken cancellationToken = default)
    {
        var index = await GetIndex(cancellationToken);
        var id = idSelector(document);
        var response = await client.IndexAsync(
            document,
            descriptor => descriptor
                .Index(index)
                .Id(id.ToString()),
            cancellationToken);

        EnsureResponseSucceeded(
            response,
            $"upsert '{id}' into");
    }

    public async Task UpsertMany(
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default)
    {
        var distinctDocuments = documents
            .DistinctBy(idSelector)
            .ToArray();
        if (distinctDocuments.Length == 0) return;

        var index = await GetIndex(cancellationToken);
        var response = await client.BulkAsync(
            descriptor => descriptor
                .Index(index)
                .IndexMany(
                    distinctDocuments,
                    (operation, document) => operation
                        .Id(idSelector(document).ToString())),
            cancellationToken);

        EnsureBulkSucceeded(response, "upsert into");
    }

    public async Task Delete(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var index = await GetIndex(cancellationToken);
        var response = await client.DeleteAsync<TDocument>(
            id.ToString(),
            descriptor => descriptor.Index(index),
            cancellationToken);

        if (response.ApiCall?.HttpStatusCode == 404) return;

        EnsureResponseSucceeded(
            response,
            $"delete '{id}' from");
    }

    public async Task DeleteMany(
        IEnumerable<TKey> ids,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = ids.Distinct().ToArray();
        if (distinctIds.Length == 0) return;

        var index = await GetIndex(cancellationToken);
        var response = await client.BulkAsync(
            descriptor =>
            {
                descriptor.Index(index);

                foreach (var id in distinctIds)
                    descriptor.Delete<TDocument>(operation => operation
                        .Id(id.ToString()));

                return descriptor;
            },
            cancellationToken);

        EnsureBulkSucceeded(response, "delete from");
    }

    protected async Task<string> GetIndex(
        CancellationToken cancellationToken = default)
    {
        await indexInitializer.LazyInitialize(cancellationToken);
        return indexProvider();
    }

    private void EnsureBulkSucceeded(
        BulkResponse response,
        string operation)
    {
        if (response.IsValid && !response.Errors) return;

        var itemErrors = string.Join(
            "; ",
            response.ItemsWithErrors.Select(item =>
                $"{item.Id}: {item.Error?.Reason ?? item.Status.ToString()}"));
        var details = string.IsNullOrWhiteSpace(itemErrors)
            ? GetErrorDetails(response)
            : itemErrors;

        throw new InvalidOperationException(
            $"Failed to {operation} OpenSearch index '{indexProvider()}'. {details}");
    }

    private void EnsureResponseSucceeded(
        IResponse response,
        string operation)
    {
        if (response.IsValid) return;

        throw new InvalidOperationException(
            $"Failed to {operation} OpenSearch index '{indexProvider()}'. {GetErrorDetails(response)}");
    }

    private static string GetErrorDetails(IResponse response)
    {
        return response.ServerError?.Error?.Reason ?? response.DebugInformation;
    }
}
