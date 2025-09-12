using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IArticlesRepository
{
    Task<List<Article>> GetArticlesByName(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<List<Article>> GetArticlesByNameOrNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<List<Article>> GetArticlesByExecNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);

    Task<List<Article>> GetArticlesByStartNumber(string searchTerm, int page, int viewCount,
        string? sortBy, IEnumerable<int> producerIds, CancellationToken cancellationToken = default);
    Task<Article?> GetArticleById(int id, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<Article>> GetArticleCrosses(int articleId, int page, int viewCount, string? sortBy,
        bool track = true, CancellationToken cancellationToken = default);
    
    Task AddArticleLinkage(IEnumerable<(int id, int crossId)> crossIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> GetArticleCrossIds(int articleId, CancellationToken cancellationToken = default);
    
    Task<List<Article>> GetArticlesByIds(IEnumerable<int> ids, bool track = true, CancellationToken token = default);
    Task<List<Article>> GetArticlesCrosses(int articleId, int page = -1, int viewCount = -1, CancellationToken token = default);
    Task<IEnumerable<Article>> GetArticlesForUpdate(IEnumerable<int> articleIds, bool track = true, CancellationToken cancellationToken = default);
    
    Task<int> UpdateArticlesCount(Dictionary<int, int> toIncrement, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<int>> ArticlesExistsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}