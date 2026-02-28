using Search.Enums;
using Search.Persistence.IndexContexts;

namespace Search.Persistence.Interfaces;

public interface IIndexManager
{
    IndexContext GetContext(IndexName indexName);
}