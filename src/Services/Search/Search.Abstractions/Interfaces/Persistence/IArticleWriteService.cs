using Search.Entities;

namespace Search.Abstractions.Interfaces.Persistence;

public interface IArticleWriteService
{
    void Add(Product product);
    void AddRange(IEnumerable<Product> articles);
    void Delete(int articleId);
}