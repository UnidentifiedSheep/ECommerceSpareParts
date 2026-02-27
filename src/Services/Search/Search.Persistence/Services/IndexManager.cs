using System.Collections.Concurrent;
using Lucene.Net.Analysis.Standard;
using Search.Enums;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.IndexDirectory;
using Search.Persistence.Models;

namespace Search.Persistence.Services;

public sealed class IndexManager : IIndexManager, IDisposable
{
    private readonly IIndexDirectoryProvider _directoryProvider;
    private readonly StandardAnalyzer _analyzer;
    private readonly ConcurrentDictionary<IndexName, IndexContext> _indexContexts = new();
    
    public IndexManager(IIndexDirectoryProvider directoryProvider, StandardAnalyzer analyzer)
    {
        _directoryProvider = directoryProvider;
        _analyzer = analyzer;
    }
    
    public IndexContext GetContext(IndexName indexName)
    {
        return _indexContexts.GetOrAdd(indexName, name => new IndexContext(_analyzer, _directoryProvider.GetDirectory(name)));
    }

    public void Dispose()
    {
        foreach (var context in _indexContexts.Values) context.Dispose();
    }
}