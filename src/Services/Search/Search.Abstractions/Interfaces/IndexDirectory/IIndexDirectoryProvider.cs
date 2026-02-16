using Lucene.Net.Store;
using Search.Enums;

namespace Search.Abstractions.Interfaces.IndexDirectory;

public interface IIndexDirectoryProvider
{
    FSDirectory GetDirectory(IndexName indexName);
}