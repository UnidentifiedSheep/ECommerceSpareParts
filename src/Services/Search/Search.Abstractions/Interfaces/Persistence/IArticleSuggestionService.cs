using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleSuggestionService
{
    List<Article> GetSuggestions(string query, int max = 10);
    Task RebuildSuggestions();
    bool IsRebuildingNow();
}