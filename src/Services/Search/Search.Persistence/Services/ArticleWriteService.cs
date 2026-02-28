using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

internal class ArticleWriteService(IArticleWriteRepository writeRepository) : IArticleWriteService
{
    public void Add(Article article) => writeRepository.Add(article);
    public void AddRange(IEnumerable<Article> articles) => writeRepository.AddRange(articles);
    public void Delete(int articleId) => writeRepository.Delete(articleId);
}