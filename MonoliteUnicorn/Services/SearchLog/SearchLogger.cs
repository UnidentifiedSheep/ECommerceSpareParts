using System.Collections.Concurrent;
using MonoliteUnicorn.Models;

namespace MonoliteUnicorn.Services.SearchLog;

public class SearchLogger : ISearchLogger
{
    private readonly ConcurrentQueue<SearchLogModel> _logs = new();
    private readonly SemaphoreSlim _trigger = new(0, 1);
    
    public int LogsCount => _logs.Count;
    
    public void Enqueue(SearchLogModel log)
    {
        _logs.Enqueue(log);

        if (_logs.Count >= 1000 && _trigger.CurrentCount == 0)
            _trigger.Release();
    }

    public bool TryDequeue(out SearchLogModel? log) => _logs.TryDequeue(out log);
    
    public async Task WaitForTriggerOrTimeoutAsync(TimeSpan timeout, CancellationToken token) 
        => await _trigger.WaitAsync(timeout, token);
    
}