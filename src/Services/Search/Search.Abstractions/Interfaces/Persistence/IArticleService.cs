using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleService
{
    void Add(Article article);
    void AddRange(IEnumerable<Article> articles);
    Article? GetArticle(int articleId);
    IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds);
    void Delete(int articleId);
}