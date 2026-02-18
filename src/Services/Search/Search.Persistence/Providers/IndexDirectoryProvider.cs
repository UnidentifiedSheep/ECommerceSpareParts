using System.Collections.Concurrent;
using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Interfaces.IndexDirectory;

namespace Search.Persistence.Providers;

public class IndexDirectoryProvider : IIndexDirectoryProvider
{
    private readonly IIndexDirectory _indexDirectory;
    private readonly ConcurrentDictionary<IndexName, FSDirectory> _directories = new();

    public IndexDirectoryProvider(IIndexDirectory indexDirectory)
    {
        _indexDirectory = indexDirectory ?? throw new ArgumentNullException(nameof(indexDirectory));
    }

    public FSDirectory GetDirectory(IndexName indexName)
    {
        return _directories.GetOrAdd(indexName, name =>
        {
            var path = _indexDirectory.GetIndexPath(name);
            return FSDirectory.Open(path);
        });
    }
}