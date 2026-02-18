using Search.Enums;

namespace Search.Persistence.Interfaces.IndexDirectory;

public interface IIndexDirectory
{
    string GetIndexPath(IndexName indexName);
}