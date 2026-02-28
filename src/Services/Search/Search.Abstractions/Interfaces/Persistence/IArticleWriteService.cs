using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleWriteService
{
    void Add(Article article);
    void AddRange(IEnumerable<Article> articles);
    void Delete(int articleId);
}