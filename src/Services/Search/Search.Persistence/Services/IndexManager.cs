using System.Collections.Concurrent;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Standard;
using Search.Enums;
using Search.Persistence.Analyzers;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.IndexDirectory;

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

    public void Dispose()
    {
        foreach (var context in _indexContexts.Values) context.Dispose();
    }
}