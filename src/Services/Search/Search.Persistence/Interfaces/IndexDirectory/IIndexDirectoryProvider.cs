using Lucene.Net.Store;
using Search.Enums;

namespace Search.Persistence.Interfaces.IndexDirectory;

public interface IIndexDirectoryProvider
{
    FSDirectory GetDirectory(IndexName indexName);
}