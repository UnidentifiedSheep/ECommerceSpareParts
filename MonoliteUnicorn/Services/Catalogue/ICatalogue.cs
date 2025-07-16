using MonoliteUnicorn.PostGres.Main;

namespace MonoliteUnicorn.Services.Catalogue;

public interface ICatalogue
{
    Task<IEnumerable<Article>> GetArticlesByName(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<Article>> GetArticlesByNameOrNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<Article>> GetArticlesByExecNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<IEnumerable<Article>> GetArticlesByStartNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task SetArticleIndicator(int articleId, string? indicator, CancellationToken token = default);

    Task AddArticlesContent(int articleId, Dictionary<int, int> content, CancellationToken token = default);
    Task RemoveArticlesContent(int articleId, IEnumerable<int> insideIds, CancellationToken token = default);

    Task SetArticleContentCount(int articleId, int insideArticleId, int count, CancellationToken token = default);
}