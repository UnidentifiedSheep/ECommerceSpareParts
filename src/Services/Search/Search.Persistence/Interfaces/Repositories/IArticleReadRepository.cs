using Search.Entities;
using Search.Persistence.Enumerators;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleReadRepository
{
    Article? GetArticle(int articleId);

    /// <summary>
    /// Retrieves the next article in the sequence after the specified article identifier.
    /// </summary>
    /// <param name="articleId">
    /// The identifier of the current article. If not provided, the sequence starts from the beginning.
    /// </param>
    /// <returns>
    /// The next article in the sequence, or <c>null</c> if no more articles are available.
    /// </returns>
    Article? GetNextArticle(int articleId = -1);

    IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds);
    IReadOnlyList<Article> SearchByTitle(string title, int lastArticleId = -1, int limit = 20);
    ArticleEnumerator GetEnumerator();

    /// <summary>
    /// Searches for articles whose article numbers start with the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match the beginning of article numbers against.</param>
    /// <param name="lastArticleId">
    /// The <c>id</c> of the last article in the current result set.
    /// If provided, the search will return results starting after this article <c>id</c>.
    /// </param>
    /// <param name="limit">
    /// The maximum number of articles to return in the search result.
    /// </param>
    /// <returns>
    /// The read only list of articles.
    /// </returns>
    IReadOnlyList<Article> SearchByArticleNumberPrefix(string prefix, int lastArticleId = -1, int limit = 20);
}