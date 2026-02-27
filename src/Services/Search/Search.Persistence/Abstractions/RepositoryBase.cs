using Search.Enums;
using Search.Persistence.Interfaces;
using Search.Persistence.Models;

namespace Search.Persistence.Abstractions;

internal abstract class RepositoryBase(IIndexManager indexManager, IndexName indexName)
{
    public IndexName IndexName { get; } = indexName;
    protected IndexContext IndexContext { get; } = indexManager.GetContext(indexName);
}
