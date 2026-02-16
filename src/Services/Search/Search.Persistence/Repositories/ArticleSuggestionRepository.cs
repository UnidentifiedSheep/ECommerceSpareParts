using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Store;
using Search.Abstractions.Interfaces.IndexDirectory;
using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Enumerators;

namespace Search.Persistence.Repositories;

public class ArticleSuggestionRepository : IArticleSuggestionRepository
{
    private const IndexName IndexName = Enums.IndexName.Article_Suggestions;
    private readonly StandardAnalyzer _analyzer;
    private readonly FSDirectory _directory;
    private readonly AnalyzingInfixSuggester _suggester;

    public ArticleSuggestionRepository(IIndexDirectoryProvider directoryProvider, StandardAnalyzer analyzer)
    {
        _analyzer = analyzer;
        _directory = directoryProvider.GetDirectory(IndexName);
        _suggester = new AnalyzingInfixSuggester(Global.LuceneVersion, _directory, _analyzer);
    }


    public int[] GetSuggestions(string query, int max = 10)
    {
        return _suggester.DoLookup(query, false, max)
            .Select(r => int.Parse(r.Payload.Utf8ToString()))
            .ToArray();
    }

    public void RebuildSuggestions(IEnumerable<Article> articles)
    {
        var enumerator = new ArticleInputEnumerator(articles);
        _suggester.Build(enumerator);
    }
}