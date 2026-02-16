using Search.Abstractions.Interfaces.IndexDirectory;
using Search.Enums;

namespace Search.Persistence.Providers;

public class IndexDirectory(string indexDirectory) : IIndexDirectory
{
    public string GetIndexPath(IndexName indexName)
    {
        return Path.Combine(indexDirectory, indexName.ToString().ToLowerInvariant());
    }
}