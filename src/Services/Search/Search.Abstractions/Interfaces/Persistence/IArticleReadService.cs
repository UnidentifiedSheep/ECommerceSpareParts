using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleReadService
{
    Product? GetArticle(int articleId);
    IReadOnlyList<Product> GetArticles(IEnumerable<int> articleIds);
    (IReadOnlyList<Product> result, string? cursor) SearchByTitle(string title, string? cursor = null, int limit = 20);

    (IReadOnlyList<Product> result, string? cursor) SearchByArticleNumberPrefix(
        string prefix,
        string? cursor = null,
        int limit = 20);
}