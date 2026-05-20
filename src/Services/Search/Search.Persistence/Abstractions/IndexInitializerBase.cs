using OpenSearch.Client;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Abstractions;

public abstract class IndexInitializerBase<TDocument>(
    IOpenSearchClient openSearchClient,
    TimeSpan durationBetweenCheck) : IIndexInitializer<TDocument>
{
    protected IOpenSearchClient Client => openSearchClient;
    protected DateTime? ExistsCheckedAt { get; private set; }
    protected bool LastExistsResult { get; private set; }
    
    public abstract Task LazyInitialize(CancellationToken cancellationToken = default);

    protected async Task<bool> CheckIfIndexExists(
        string idx,
        CancellationToken cancellationToken = default)
    {
        if (!ExistsCheckedAt.HasValue || (ExistsCheckedAt + durationBetweenCheck) < DateTime.UtcNow)
        {
            LastExistsResult = (await Client.Indices.ExistsAsync(idx, ct: cancellationToken))
                .Exists;
            
            ExistsCheckedAt = DateTime.UtcNow;
        }

        return LastExistsResult;
    }
}