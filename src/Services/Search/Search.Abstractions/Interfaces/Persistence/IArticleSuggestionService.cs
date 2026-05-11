using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleSuggestionService
{
    IReadOnlyList<Product> GetSuggestions(string query, int max = 10);
    Task RebuildSuggestions();
    bool IsRebuildingNow();
}