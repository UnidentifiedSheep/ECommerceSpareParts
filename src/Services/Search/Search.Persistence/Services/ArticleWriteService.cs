using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

internal class ArticleWriteService(IArticleWriteRepository writeRepository) : IArticleWriteService
{
    public void Add(Product product)
    {
        writeRepository.Add(product);
    }

    public void AddRange(IEnumerable<Product> articles)
    {
        writeRepository.AddRange(articles);
    }

    public void Delete(int articleId)
    {
        writeRepository.Delete(articleId);
    }
}