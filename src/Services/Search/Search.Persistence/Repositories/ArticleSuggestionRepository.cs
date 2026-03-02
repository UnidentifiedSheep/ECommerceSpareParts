using Lucene.Net.Search.Suggest;
using Lucene.Net.Search.Suggest.Analyzing;
using Lucene.Net.Store;
using Search.Enums;
using Search.Persistence.Abstractions;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.IndexDirectory;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Repositories;

internal sealed class ArticleSuggestionRepository(IIndexManager indexManager, IIndexDirectory indexDir)
    : RepositoryBase<ArticleSuggestionsContext>(indexManager, IndexName.Article_Suggestions), IArticleSuggestionRepository
{
    public int[] GetSuggestions(string query, int max = 10)
    {
        var lookupResult = IndexContext.Suggester.DoLookup(query, false, max);
        if (lookupResult == null) return [];
        
        var result = lookupResult
            .Select(r => int.Parse(r.Payload.Utf8ToString()))
            .ToArray();
        
        return result;
    }

    public DirectoryInfo RebuildSuggestions(IInputEnumerator enumerator)
    {
        using var temp = FSDirectory.Open(indexDir.GetTempPath());
        using var newSuggester = new AnalyzingInfixSuggester(Global.LuceneVersion, temp, IndexContext.Analyzer);
        newSuggester.Build(enumerator);
        return temp.Directory;
    }
}