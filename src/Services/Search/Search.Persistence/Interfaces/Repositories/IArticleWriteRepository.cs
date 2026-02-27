using Search.Entities;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleWriteRepository
{
    void Add(Article article);
    void AddRange(IEnumerable<Article> articles);
    void Delete(int articleId);
}