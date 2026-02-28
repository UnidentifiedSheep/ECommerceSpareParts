using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleReadService
{
    Article? GetArticle(int articleId);
    IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds);
    (IReadOnlyList<Article> result, string? cursor) SearchByTitle(string title, string? cursor = null, int limit = 20);
    (IReadOnlyList<Article> result, string? cursor) SearchByArticleNumberPrefix(string prefix, string? cursor = null, int limit = 20);
}