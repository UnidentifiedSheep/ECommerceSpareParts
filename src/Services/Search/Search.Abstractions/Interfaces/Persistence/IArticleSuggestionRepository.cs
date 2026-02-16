using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleSuggestionRepository
{
    int[] GetSuggestions(string query, int max = 10);
    void RebuildSuggestions(IEnumerable<Article> articles);
}