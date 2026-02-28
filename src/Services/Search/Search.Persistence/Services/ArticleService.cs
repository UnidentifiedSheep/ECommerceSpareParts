using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

public class ArticleService(IArticleWriteRepository writeRepository, IArticleReadRepository readRepository) : IArticleService
{
    public void Add(Article article) => writeRepository.Add(article);
    public void AddRange(IEnumerable<Article> articles) => writeRepository.AddRange(articles);
    public void Delete(int articleId) => writeRepository.Delete(articleId);
    public Article? GetArticle(int articleId) => readRepository.GetArticle(articleId);
    public IReadOnlyList<Article> GetArticles(IEnumerable<int> articleIds) => readRepository.GetArticles(articleIds);
}