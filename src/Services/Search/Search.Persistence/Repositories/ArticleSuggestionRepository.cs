using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Search.Entities;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.Interfaces.IndexDirectory;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal class ArticleSuggestionRepository : RepositoryBase, IArticleSuggestionRepository
{
    private readonly AnalyzingInfixSuggester _suggester;

    public ArticleSuggestionRepository(IIndexDirectoryProvider directoryProvider, StandardAnalyzer analyzer) 
        : base(directoryProvider, analyzer, IndexName.Article_Suggestions)
    {
        _suggester = new AnalyzingInfixSuggester(Global.LuceneVersion, Directory, Analyzer);
    }


    public int[] GetSuggestions(string query, int max = 10)
    {
        return _suggester.DoLookup(query, false, max)
            .Select(r => int.Parse(r.Payload.Utf8ToString()))
            .ToArray();
    }

    public void RebuildSuggestions(IInputEnumerator enumerator)
    {
        _suggester.Build(enumerator);
    }
    
    public override void Dispose()
    {
        _suggester.Dispose();
        base.Dispose();
    }
}