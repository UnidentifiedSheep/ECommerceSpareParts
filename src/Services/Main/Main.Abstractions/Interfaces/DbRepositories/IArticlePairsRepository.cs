using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticlePairsRepository
{
    Task<Product?> GetArticlePairAsync(int articleId, bool track = true, CancellationToken cancellationToken = default);

    Task<IEnumerable<ProductPair>> GetRelatedPairsAsync(
        int articleId,
        bool track = true,
        CancellationToken cancellationToken = default);
}