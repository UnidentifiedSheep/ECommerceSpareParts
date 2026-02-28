using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Analyzers;

namespace Search.Persistence.IndexContexts;

public class ArticleIndexContext(ArticleAnalyzer analyzer, FSDirectory directory) : IndexContext(analyzer, directory)
{
    public override IndexName IndexName => IndexName.Articles;
}