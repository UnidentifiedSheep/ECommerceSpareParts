using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleSuggestionService
{
    IReadOnlyList<Article> GetSuggestions(string query, int max = 10);
    Task RebuildSuggestions();
    bool IsRebuildingNow();
}