using System.Linq.Expressions;
using MonoliteUnicorn.Dtos.Amw.Articles;
using MonoliteUnicorn.PostGres.Main;
using StackExchange.Redis;

namespace MonoliteUnicorn.RedisFunctions;

public interface IRedisArticleRepository
{
    Task SetArticles(IEnumerable<Article> articles);
    Task<Dictionary<int, ArticleFullDto?>> GetArticles(IEnumerable<int> articleIds);
    Task AddArticleCrosses(int articleId, IEnumerable<int> crosses);
    Task<HashSet<int>> GetArticleCrosses(int articleId, int offset = 0, int limit = 100);
    Task RemoveArticleCrosses(int articleId, IEnumerable<int> crossIds);
    Task ClearUsableArticleData(int articleId);
    Task AddArticleCrossesWithSort(int articleId, SortedSetEntry[] entries, Expression<Func<ArticleFullDto, object>> sortFieldSelector);
    Task<HashSet<int>> GetArticleCrossesWithSort(int articleId, Expression<Func<ArticleFullDto, object>> sortFieldSelector, int offset = 0, int limit = 100, string sort = "desc");
    Task RemoveArticleCrossesWithSort(int articleId, IEnumerable<int> crossIds, Expression<Func<ArticleFullDto, object>> sortFieldSelector);
    Task UpdateArticlesWhichContains(IEnumerable<Article> articles);
}