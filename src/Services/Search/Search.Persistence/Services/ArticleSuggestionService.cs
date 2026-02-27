using Exceptions.Exceptions.Suggestions;
using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

public class ArticleSuggestionService(IArticleReadRepository readRepository, 
    IArticleSuggestionRepository suggestionRepository) : IArticleSuggestionService
{
    private static readonly SemaphoreSlim RebuildLock = new(1, 1);
    
    public List<Article> GetSuggestions(string query, int max = 10)
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
            suggestionRepository.RebuildSuggestions(enumerator);
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