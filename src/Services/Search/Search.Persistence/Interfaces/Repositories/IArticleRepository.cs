using Search.Entities;
using Search.Persistence.Enumerators;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleRepository
{
    void Add(Article article);
    void AddRange(IEnumerable<Article> articles);
    Article? GetArticle(int articleId);
    List<Article> GetArticles(IEnumerable<int> articleIds);
    ArticleEnumerator GetEnumerator();
    Article? GetNextArticle(int articleId);
    void Delete(int articleId);
}