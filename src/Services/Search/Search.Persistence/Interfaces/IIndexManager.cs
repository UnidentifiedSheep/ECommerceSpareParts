using Search.Enums;
using Search.Persistence.Models;

namespace Search.Persistence.Interfaces;

public interface IIndexManager
{
    IndexContext GetContext(IndexName indexName);
}