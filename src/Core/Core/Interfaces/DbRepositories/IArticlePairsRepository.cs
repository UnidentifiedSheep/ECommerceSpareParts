using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IArticlePairsRepository
{
    Task<Article?> GetArticlePairAsync(int articleId, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<ArticlesPair>> GetRelatedPairsAsync(int articleId, bool track = true,
        CancellationToken cancellationToken = default);
}