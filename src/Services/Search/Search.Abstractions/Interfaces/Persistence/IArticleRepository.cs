using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleRepository
{
    void Add(Article article);
    void AddRange(IEnumerable<Article> articles);
    Article? GetArticle(int articleId);
    List<Article> GetArticles(IEnumerable<int> articleIds);
}