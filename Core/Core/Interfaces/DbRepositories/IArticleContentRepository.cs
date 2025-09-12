using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IArticleContentRepository
{
    Task<IEnumerable<ArticlesContent>> GetArticleContents(int articleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<ArticlesContent?> GetArticleContentAsync(int articleId, int insideArticleId, bool track = true,
        CancellationToken cancellationToken = default);
}