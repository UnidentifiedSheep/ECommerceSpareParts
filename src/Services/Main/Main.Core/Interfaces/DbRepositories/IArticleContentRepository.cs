using Main.Core.Entities;

namespace Main.Core.Interfaces.DbRepositories;

public interface IArticleContentRepository
{
    Task<IEnumerable<ArticlesContent>> GetArticleContents(int articleId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<ArticlesContent?> GetArticleContent(int articleId, int insideArticleId, bool track = true,
        CancellationToken cancellationToken = default);
}