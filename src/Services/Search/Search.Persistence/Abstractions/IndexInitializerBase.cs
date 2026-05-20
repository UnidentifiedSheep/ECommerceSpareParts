using OpenSearch.Client;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Abstractions;

public abstract class IndexInitializerBase<TDocument>(
    IOpenSearchClient openSearchClient,
    TimeSpan durationBetweenCheck) : IIndexInitializer<TDocument>
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    protected IOpenSearchClient Client => openSearchClient;
    protected DateTime? ExistsCheckedAt { get; private set; }
    protected bool LastExistsResult { get; private set; }

    public abstract Task LazyInitialize(CancellationToken cancellationToken = default);

    protected async Task InitializeIfMissing(
        string idx,
        Func<CancellationToken, Task<CreateIndexResponse>> createIndex,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldRefreshExistsCache() && LastExistsResult)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (!ShouldRefreshExistsCache() && LastExistsResult)
            {
                return;
            }

            LastExistsResult = (await Client.Indices.ExistsAsync(idx, ct: cancellationToken)).Exists;
            ExistsCheckedAt = DateTime.UtcNow;

            if (LastExistsResult)
            {
                return;
            }

            var response = await createIndex(cancellationToken);
            var alreadyExists = response.ServerError?.Error?.Type == "resource_already_exists_exception";

            if (!response.IsValid && !alreadyExists)
            {
                throw new InvalidOperationException(
                    response.ServerError?.Error?.Reason ?? $"Failed to create OpenSearch index '{idx}'.");
            }

            LastExistsResult = true;
            ExistsCheckedAt = DateTime.UtcNow;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private bool ShouldRefreshExistsCache()
    {
        return !ExistsCheckedAt.HasValue ||
               ExistsCheckedAt.Value + durationBetweenCheck < DateTime.UtcNow;
    }
}
