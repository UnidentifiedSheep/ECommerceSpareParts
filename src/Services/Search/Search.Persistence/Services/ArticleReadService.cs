using Search.Abstractions.Interfaces.Persistence;
using Search.Abstractions.Models;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

internal class ArticleReadService(IArticleReadRepository readRepository) : IArticleReadService
{
    public Article? GetArticle(int articleId)
    {
        return readRepository.GetArticle(articleId);
    }

    public IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds)
    {
        return readRepository.GetArticles(articleIds);
    }

    public (IReadOnlyList<Article> result, string? cursor) SearchByTitle(
        string title,
        string? cursor = null,
        int limit = 20)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        var cursorModel = SearchCursor.DecodeCursor(cursor);

        var (result, newCursor) = readRepository.SearchByTitle(title, cursorModel, limit);
        return (result, newCursor?.EncodeCursor());
    }

    public (IReadOnlyList<Article> result, string? cursor) SearchByArticleNumberPrefix(
        string prefix,
        string? cursor = null,
        int limit = 20)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        var cursorModel = SearchCursor.DecodeCursor(cursor);

        var (result, newCursor) = readRepository.SearchByArticleNumberPrefix(prefix, cursorModel, limit);
        return (result, newCursor?.EncodeCursor());
    }
}