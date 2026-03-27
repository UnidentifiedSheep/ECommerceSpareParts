using Search.Enums;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Abstractions;

internal abstract class RepositoryBase<TContext>(IIndexManager indexManager, IndexName indexName)
    where TContext : IndexContext
{
    public IndexName IndexName { get; } = indexName;
    protected TContext IndexContext { get; set; } = indexManager.GetContext<TContext>(indexName);
}