using Main.Entities;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticlePairsRepository
{
    Task<Product?> GetArticlePairAsync(int articleId, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<ArticlesPair>> GetRelatedPairsAsync(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default);
}