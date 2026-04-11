using Main.Entities;
using Main.Entities.Product;

namespace Main.Abstractions.Interfaces.DbRepositories;

public interface IArticleImageRepository
{
    Task<IEnumerable<ProductImage>> GetArticlesImages(
        IEnumerable<int> articleIds,
        bool track = true,
        CancellationToken cancellationToken = default);
}