using System.Collections.Concurrent;
using Search.Enums;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Services;

internal sealed class IndexManager : IIndexManager, IDisposable
{
    private readonly ConcurrentDictionary<IndexName, IndexContext> _indexContexts;
    
    public IndexManager(IEnumerable<IndexContext> contexts)
    {
        _indexContexts = new ConcurrentDictionary<IndexName, IndexContext>();
        foreach (var context in contexts) _indexContexts.TryAdd(context.IndexName, context);
    }
    
    public IndexContext GetContext(IndexName indexName)
    {
        if (_indexContexts.TryGetValue(indexName, out var value))
            return value;
        throw new KeyNotFoundException($"Index {indexName} not found. Before getting context, you must add it to the manager.");
    }

    public TContext GetContext<TContext>(IndexName indexName) where TContext : IndexContext
    {
        var ctx = GetContext(indexName);
        if (ctx is TContext typedCtx) return typedCtx;
        throw new InvalidCastException($"Context {ctx.GetType().Name} is not of type {typeof(TContext).Name}");
    }

    public void Dispose()
    {
        foreach (var context in _indexContexts.Values) context.Dispose();
    }
}