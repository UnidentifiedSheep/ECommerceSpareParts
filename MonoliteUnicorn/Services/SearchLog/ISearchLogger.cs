using MonoliteUnicorn.Models;

namespace MonoliteUnicorn.Services.SearchLog;

public interface ISearchLogger
{
    void Enqueue(SearchLogModel log);
    bool TryDequeue(out SearchLogModel? log);
    Task WaitForTriggerOrTimeoutAsync(TimeSpan timeout, CancellationToken token);
    int LogsCount { get; }
}