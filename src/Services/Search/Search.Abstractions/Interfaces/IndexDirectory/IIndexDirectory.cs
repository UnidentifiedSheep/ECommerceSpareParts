using Search.Enums;

namespace Search.Abstractions.Interfaces.IndexDirectory;

public interface IIndexDirectory
{
    string GetIndexPath(IndexName indexName);
}