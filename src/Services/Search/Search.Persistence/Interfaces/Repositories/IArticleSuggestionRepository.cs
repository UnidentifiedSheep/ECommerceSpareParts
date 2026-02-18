using Lucene.Net.Search.Suggest;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleSuggestionRepository
{
    int[] GetSuggestions(string query, int max = 10);
    void RebuildSuggestions(IInputEnumerator enumerator);
}