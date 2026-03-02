using Exceptions.Exceptions.Suggestions;
using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Enums;
using Search.Persistence.IndexContexts;
using Search.Persistence.Interfaces;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

internal class ArticleSuggestionService(IArticleReadRepository readRepository, 
    IArticleSuggestionRepository suggestionRepository, IIndexManager indexManager) : IArticleSuggestionService
{
    private static readonly SemaphoreSlim RebuildLock = new(1, 1);
    
    public IReadOnlyList<Article> GetSuggestions(string query, int max = 10)
    {
        var ids = suggestionRepository.GetSuggestions(query, max);
        return readRepository.GetArticles(ids);
    }

    public async Task RebuildSuggestions()
    {
        if (!await RebuildLock.WaitAsync(10))
            throw new SuggestionsRebuildingException();
        try
        {
            using var enumerator = readRepository.GetEnumerator();
            var newIndexDir = suggestionRepository.RebuildSuggestions(enumerator);
            var ctx = indexManager.GetContext<ArticleSuggestionsContext>(IndexName.Article_Suggestions);
            var mainDir = ctx.Directory.Directory;
            ctx.Close();

            // Delete the old index directory and replace it with a new one
            if (mainDir.Exists) mainDir.Delete(true);
            newIndexDir.MoveTo(mainDir.FullName);
            
            // Reopen the ctx
            ctx.Open();
        }
        finally
        {
            RebuildLock.Release();
        }
    }

    public bool IsRebuildingNow()
    {
        return RebuildLock.CurrentCount == 0;
    }
}