using Search.Entities;

namespace Search.Persistence.Interfaces.Repositories;

public interface IArticleWriteRepository
{
    void Add(Product product);
    void AddRange(IEnumerable<Product> articles);
    void Delete(int articleId);
}