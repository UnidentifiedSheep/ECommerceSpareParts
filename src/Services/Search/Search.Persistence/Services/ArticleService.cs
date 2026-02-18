using Search.Abstractions.Interfaces.Persistence;
using Search.Entities;
using Search.Persistence.Interfaces.Repositories;

namespace Search.Persistence.Services;

public class ArticleService(IArticleRepository articleRepository) : IArticleService
{
    public void Add(Article article) => articleRepository.Add(article);
    
    public void AddRange(IEnumerable<Article> articles) => articleRepository.AddRange(articles);

    public Article? GetArticle(int articleId) => articleRepository.GetArticle(articleId);

    public List<Article> GetArticles(IEnumerable<int> articleIds) => articleRepository.GetArticles(articleIds);
    public void Delete(int articleId) => articleRepository.Delete(articleId);
}