using Lucene.Net.Search.Suggest;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleSuggestionRepository
{
    int[] GetSuggestions(string query, int max = 10);
    /// <summary>
    /// Rebuilds the suggestions index and returns the directory info of the new index.
    /// </summary>
    /// <param name="enumerator">Enumerator to take information from.</param>
    /// <returns>Directory info of the new index.</returns>
    DirectoryInfo RebuildSuggestions(IInputEnumerator enumerator);
}