using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ISearchLogger
{
    int LogsCount { get; }
    void Enqueue(SearchLogModel log);
    bool TryDequeue(out SearchLogModel? log);
    Task WaitForTriggerOrTimeoutAsync(TimeSpan timeout, CancellationToken token);
}