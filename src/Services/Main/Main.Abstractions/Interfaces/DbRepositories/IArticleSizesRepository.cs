using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleSizesRepository
{
    Task<ProductSize?> GetArticleSizes(int articleId, bool track = true, CancellationToken token = default);

    Task<IEnumerable<ProductSize>> GetArticleSizesByIds(
        IEnumerable<int> ids,
        bool track = true,
        CancellationToken token = default);
}